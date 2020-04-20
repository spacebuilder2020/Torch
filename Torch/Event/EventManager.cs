using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using Torch.Managers;

namespace Torch.Event
{
    /// <summary>
    ///     Manager class responsible for managing registration and dispatching of events.
    /// </summary>
    public class EventManager : Manager, IEventManager
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<Type, IEventList> _eventLists = new Dictionary<Type, IEventList>();

        private static readonly HashSet<Type> _dispatchShims = new HashSet<Type>();

        private readonly Dictionary<Assembly, HashSet<IEventHandler>> _registeredHandlers = new Dictionary<Assembly, HashSet<IEventHandler>>();

        /// <inheritdoc />
        public EventManager(ITorchBase torchInstance) : base(torchInstance) { }

        /// <summary>
        ///     Registers all event handler methods contained in the given instance
        /// </summary>
        /// <param name="handler">Instance to register</param>
        /// <returns><b>true</b> if added, <b>false</b> otherwise</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool RegisterHandler(IEventHandler handler)
        {
            var caller = Assembly.GetCallingAssembly();
            lock (_registeredHandlers)
            {
                if (!_registeredHandlers.TryGetValue(caller, out var handlers))
                    _registeredHandlers.Add(caller, handlers = new HashSet<IEventHandler>());
                if (handlers.Add(handler))
                {
                    RegisterHandlerInternal(handler);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Unregisters all event handler methods contained in the given instance
        /// </summary>
        /// <param name="handler">Instance to unregister</param>
        /// <returns><b>true</b> if removed, <b>false</b> otherwise</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool UnregisterHandler(IEventHandler handler)
        {
            var caller = Assembly.GetCallingAssembly();
            lock (_registeredHandlers)
            {
                if (!_registeredHandlers.TryGetValue(caller, out var handlers))
                    return false;

                if (handlers.Remove(handler))
                {
                    UnregisterHandlerInternal(handler);
                    return true;
                }

                return false;
            }
        }

        internal static void AddDispatchShims(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
                if (type.HasAttribute<EventShimAttribute>())
                    AddDispatchShim(type);
        }

        private static void AddDispatchShim(Type type)
        {
            lock (_dispatchShims)
                if (!_dispatchShims.Add(type))
                    return;

            if (!type.IsSealed || !type.IsAbstract)
                _log.Warn($"Registering type {type.FullName} as an event dispatch type, even though it isn't declared singleton");
            var listsFound = 0;
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(EventList<>))
                {
                    var eventType = field.FieldType.GenericTypeArguments[0];
                    if (_eventLists.ContainsKey(eventType))
                        _log.Error($"Ignore event dispatch list {type.FullName}#{field.Name}; we already have one.");
                    else
                    {
                        _eventLists.Add(eventType, (IEventList)field.GetValue(null));
                        listsFound++;
                    }
                }

            if (listsFound == 0)
                _log.Warn($"Registering type {type.FullName} as an event dispatch type, even though it has no event lists.");
        }

        /// <summary>
        ///     Gets all event handler methods declared by the given type and its base types.
        /// </summary>
        /// <param name="exploreType">Type to explore</param>
        /// <returns>All event handler methods</returns>
        private static IEnumerable<MethodInfo> EventHandlers(Type exploreType)
        {
            var enumerable = exploreType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                        .Where(x =>
                                        {
                                            var attr = x.GetCustomAttribute<EventHandlerAttribute>();
                                            if (attr == null)
                                                return false;

                                            var ps = x.GetParameters();
                                            if (ps.Length != 1)
                                                return false;

                                            return ps[0].ParameterType.IsByRef && typeof(IEvent).IsAssignableFrom(ps[0].ParameterType.GetElementType());
                                        });
            return exploreType.BaseType != null ? enumerable.Concat(EventHandlers(exploreType.BaseType)) : enumerable;
        }

        /// <inheritdoc />
        private static void RegisterHandlerInternal(IEventHandler instance)
        {
            var foundHandler = false;
            foreach (var handler in EventHandlers(instance.GetType()))
            {
                var eventType = handler.GetParameters()[0].ParameterType.GetElementType();
                Debug.Assert(eventType != null);
                foundHandler = true;
                if (eventType.IsInterface)
                {
                    var foundList = false;
                    foreach (var kv in _eventLists)
                        if (eventType.IsAssignableFrom(kv.Key))
                        {
                            kv.Value.AddHandler(handler, instance);
                            foundList = true;
                        }

                    if (foundList)
                        continue;
                }
                else if (_eventLists.TryGetValue(eventType, out var list))
                {
                    list.AddHandler(handler, instance);
                    continue;
                }

                _log.Error($"Unable to find event handler list for event type {eventType.FullName}");
            }

            if (!foundHandler)
                _log.Warn($"Found no handlers in {instance.GetType().FullName} or base types");
        }

        /// <summary>
        ///     Unregisters all handlers owned by the given instance
        /// </summary>
        /// <param name="instance">Instance</param>
        private static void UnregisterHandlerInternal(IEventHandler instance)
        {
            foreach (var list in _eventLists.Values)
                list.RemoveHandlers(instance);
        }

        /// <summary>
        ///     Unregisters all handlers owned by the given assembly.
        /// </summary>
        /// <param name="asm">Assembly to unregister</param>
        /// <param name="callback">Optional callback invoked before a handler is unregistered.  Ignored if null</param>
        /// <returns>the number of handlers that were unregistered</returns>
        internal int UnregisterAllHandlers(Assembly asm, Action<IEventHandler> callback = null)
        {
            lock (_registeredHandlers)
            {
                if (!_registeredHandlers.TryGetValue(asm, out var handlers))
                    return 0;

                foreach (var k in handlers)
                {
                    callback?.Invoke(k);
                    UnregisterHandlerInternal(k);
                }

                var count = handlers.Count;
                handlers.Clear();
                _registeredHandlers.Remove(asm);
                return count;
            }
        }
    }
}