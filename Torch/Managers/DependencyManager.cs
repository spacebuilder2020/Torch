using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Torch.API.Managers;

namespace Torch.Managers
{
    public sealed class DependencyManager : IDependencyManager
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<Type, ManagerInstance> _dependencySatisfaction;
        private readonly List<ManagerInstance> _orderedManagers;
        private readonly IDependencyProvider[] _parentProviders;
        private readonly List<ManagerInstance> _registeredManagers;

        private bool _initialized = false;

        public DependencyManager(params IDependencyProvider[] parents)
        {
            _dependencySatisfaction = new Dictionary<Type, ManagerInstance>();
            _registeredManagers = new List<ManagerInstance>();
            _orderedManagers = new List<ManagerInstance>();
            _parentProviders = parents.Distinct().ToArray();
        }

        /// <inheritdoc />
        public bool AddManager(IManager manager)
        {
            if (_initialized)
                throw new InvalidOperationException("Can't add new managers to an initialized dependency manager");

            // Protect against adding a manager derived from an existing manager
            if (_registeredManagers.Any(x => x.Instance.GetType().IsInstanceOfType(manager)))
                return false;

            // Protect against adding a manager when an existing manager derives from it.
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_registeredManagers.Any(x => manager.GetType().IsInstanceOfType(x.Instance)))
                return false;

            var instance = new ManagerInstance(manager);
            _registeredManagers.Add(instance);
            AddDependencySatisfaction(instance);
            return true;
        }

        /// <inheritdoc />
        public void ClearManagers()
        {
            if (_initialized)
                throw new InvalidOperationException("Can't remove managers from an initialized dependency manager");

            _registeredManagers.Clear();
            _dependencySatisfaction.Clear();
        }

        /// <inheritdoc />
        public bool RemoveManager(IManager manager)
        {
            if (_initialized)
                throw new InvalidOperationException("Can't remove managers from an initialized dependency manager");

            if (manager == null)
                return false;

            for (var i = 0; i < _registeredManagers.Count; i++)
                if (_registeredManagers[i].Instance == manager)
                {
                    _registeredManagers.RemoveAtFast(i);
                    RebuildDependencySatisfaction();
                    return true;
                }

            return false;
        }

        /// <summary>
        ///     Initializes the dependency manager, and all its registered managers.
        /// </summary>
        public void Attach()
        {
            if (_initialized)
                throw new InvalidOperationException("Can't start the dependency manager more than once");

            _initialized = true;
            Sort();
            foreach (var manager in _orderedManagers)
                manager.Instance.Attach();
        }

        /// <summary>
        ///     Disposes the dependency manager, and all its registered managers.
        /// </summary>
        public void Detach()
        {
            if (!_initialized)
                throw new InvalidOperationException("Can't dispose an uninitialized dependency manager");

            for (var i = _orderedManagers.Count - 1; i >= 0; i--)
            {
                _orderedManagers[i].Instance.Detach();
                foreach (var field in _orderedManagers[i].Dependencies)
                    field.Field.SetValue(_orderedManagers[i].Instance, null);
            }

            _initialized = false;
        }

        /// <inheritdoc />
        public IEnumerable<IManager> AttachOrder
        {
            get
            {
                if (!_initialized)
                    throw new InvalidOperationException("Can't determine dependency load order when uninitialized");

                foreach (var k in _orderedManagers)
                    yield return k.Instance;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IManager> DetachOrder
        {
            get
            {
                if (!_initialized)
                    throw new InvalidOperationException("Can't determine dependency load order when uninitialized");

                for (var i = _orderedManagers.Count - 1; i >= 0; i--)
                    yield return _orderedManagers[i].Instance;
            }
        }

        /// <inheritdoc />
        public IManager GetManager(Type type)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_dependencySatisfaction.TryGetValue(type, out var mgr))
                return mgr.Instance;

            foreach (var provider in _parentProviders)
            {
                var entry = provider.GetManager(type);
                if (entry != null)
                    return entry;
            }

            return null;
        }

        private void AddDependencySatisfaction(ManagerInstance instance)
        {
            foreach (var supplied in instance.SuppliedManagers)
                if (_dependencySatisfaction.ContainsKey(supplied))
                    // When we already have a manager supplying this component we have to unregister it.
                    _dependencySatisfaction[supplied] = null;
                else
                    _dependencySatisfaction.Add(supplied, instance);
        }

        private void RebuildDependencySatisfaction()
        {
            _dependencySatisfaction.Clear();
            foreach (var manager in _registeredManagers)
                AddDependencySatisfaction(manager);
        }

        private void Sort()
        {
            // Resets the previous sort results

            #region Reset

            _orderedManagers.Clear();
            foreach (var manager in _registeredManagers)
                manager.Dependents.Clear();

            #endregion

            // Creates the dependency graph

            #region Prepare

            var dagQueue = new List<ManagerInstance>();
            foreach (var manager in _registeredManagers)
            {
                var inFactor = 0;
                foreach (var dependency in manager.Dependencies)
                {
                    if (_dependencySatisfaction.TryGetValue(dependency.DependencyType, out var dependencyInstance))
                    {
                        if (dependency.Ordered)
                        {
                            inFactor++;
                            dependencyInstance.Dependents.Add(manager);
                        }
                    }
                    else if (!dependency.Optional && _parentProviders.All(x => x.GetManager(dependency.DependencyType) == null))
                        _log.Warn("Unable to satisfy dependency {0} ({1}) of {2}.", dependency.DependencyType.Name,
                            dependency.Field.Name, manager.Instance.GetType().Name);
                }

                manager.UnsolvedDependencies = inFactor;
                dagQueue.Add(manager);
            }

            #endregion

            // Sorts the dependency graph into _orderedManagers

            #region Sort

            var tmpQueue = new List<ManagerInstance>();
            while (dagQueue.Any())
            {
                tmpQueue.Clear();
                for (var i = 0; i < dagQueue.Count; i++)
                {
                    if (dagQueue[i].UnsolvedDependencies == 0)
                        tmpQueue.Add(dagQueue[i]);
                    else
                        dagQueue[i - tmpQueue.Count] = dagQueue[i];
                }

                dagQueue.RemoveRange(dagQueue.Count - tmpQueue.Count, tmpQueue.Count);
                if (tmpQueue.Count == 0)
                {
                    _log.Fatal("Dependency loop detected in the following managers:");
                    foreach (var manager in dagQueue)
                    {
                        _log.Fatal("   + {0} has {1} unsolved dependencies.", manager.Instance.GetType().FullName, manager.UnsolvedDependencies);
                        _log.Fatal("        - Dependencies: {0}",
                            string.Join(", ", manager.Dependencies.Select(x => x.DependencyType.Name + (x.Optional ? " (Optional)" : ""))));
                        _log.Fatal("        - Dependents: {0}",
                            string.Join(", ", manager.Dependents.Select(x => x.Instance.GetType().Name)));
                    }

                    throw new InvalidOperationException("Unable to satisfy all required manager dependencies");
                }

                // Update the number of unsolved dependencies
                foreach (var manager in tmpQueue)
                foreach (var dependent in manager.Dependents)
                    dependent.UnsolvedDependencies--;
                // tmpQueue.Sort(); If we have priorities this is where to sort them.
                _orderedManagers.AddRange(tmpQueue);
            }

            _log.Debug("Dependency tree satisfied.  Load order is:");
            foreach (var manager in _orderedManagers)
            {
                _log.Debug("   - {0}", manager.Instance.GetType().FullName);
                _log.Debug("        - Dependencies: {0}",
                    string.Join(", ", manager.Dependencies.Select(x => x.DependencyType.Name + (x.Optional ? " (Optional)" : ""))));
                _log.Debug("        - Dependents: {0}",
                    string.Join(", ", manager.Dependents.Select(x => x.Instance.GetType().Name)));
            }

            #endregion

            // Updates the dependency fields with the correct manager instances

            #region Satisfy

            foreach (var manager in _registeredManagers)
            {
                manager.Dependents.Clear();
                foreach (var dependency in manager.Dependencies)
                    dependency.Field.SetValue(manager.Instance, GetManager(dependency.DependencyType));
            }

            #endregion
        }

        private class DependencyInfo
        {
            private readonly Manager.DependencyAttribute _attribute;

            public DependencyInfo(FieldInfo field)
            {
                Field = field;
                _attribute = field.GetCustomAttribute<Manager.DependencyAttribute>();
            }

            internal Type DependencyType => Field.FieldType;
            internal FieldInfo Field { get; }
            internal bool Optional => _attribute.Optional;
            internal bool Ordered => _attribute.Ordered;
        }

        /// <summary>
        ///     Represents a registered instance of a manager.
        /// </summary>
        private class ManagerInstance
        {
            internal readonly List<DependencyInfo> Dependencies;
            internal readonly HashSet<ManagerInstance> Dependents;
            internal readonly HashSet<Type> SuppliedManagers;

            public ManagerInstance(IManager manager)
            {
                Instance = manager;

                SuppliedManagers = new HashSet<Type>();
                Dependencies = new List<DependencyInfo>();
                Dependents = new HashSet<ManagerInstance>();
                var openBases = new Queue<Type>();
                openBases.Enqueue(manager.GetType());
                while (openBases.TryDequeue(out var type))
                {
                    if (!SuppliedManagers.Add(type))
                        continue;

                    foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        if (field.HasAttribute<Manager.DependencyAttribute>())
                            Dependencies.Add(new DependencyInfo(field));

                    foreach (var parent in type.GetInterfaces())
                        openBases.Enqueue(parent);
                    if (type.BaseType != null)
                        openBases.Enqueue(type.BaseType);
                }
            }

            public IManager Instance { get; }

            /// <summary>
            ///     Used by <see cref="DependencyManager" /> internally to topologically sort the dependency list.
            /// </summary>
            public int UnsolvedDependencies { get; set; }
        }
    }
}