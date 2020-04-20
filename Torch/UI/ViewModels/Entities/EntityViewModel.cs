using System;
using System.Collections.Generic;
using NLog;
using Sandbox.Game.Entities;
using Torch.Collections;
using Torch.Extensions;
using Torch.Managers;
using VRage.ModAPI;
using VRageMath;

namespace Torch.UI.ViewModels.Entities
{
    public class EntityViewModel : ViewModel
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private IMyEntity _backing;

        private string _descriptiveName;

        public EntityViewModel(IMyEntity entity, EntityTreeViewModel tree)
        {
            Entity = entity;
            Tree = tree;
        }

        public EntityViewModel() { }

        protected EntityTreeViewModel Tree { get; }

        public IMyEntity Entity
        {
            get => _backing;
            protected set
            {
                _backing = value;
                OnPropertyChanged();
                EntityControls = TorchBase.Instance?.Managers.GetManager<EntityControlManager>()?.BoundModels(this);
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(EntityControls));
            }
        }

        public long Id => Entity.EntityId;

        public MtObservableList<EntityControlViewModel> EntityControls { get; private set; }

        public virtual string Name
        {
            get => Entity?.DisplayName ?? (Entity != null ? $"eid:{Entity.EntityId}" : "nil");
            set
            {
                if (Entity != null)
                    TorchBase.Instance.InvokeBlocking(() => Entity.DisplayName = value);
                OnPropertyChanged();
            }
        }

        public string DescriptiveName { get => _descriptiveName ?? (_descriptiveName = GetSortedName(EntityTreeViewModel.SortEnum.Name)); set => _descriptiveName = value; }

        public virtual string Position
        {
            get => Entity?.GetPosition().ToString();
            set
            {
                if (!Vector3D.TryParse(value, out var v))
                    return;

                if (Entity != null)
                    TorchBase.Instance.InvokeBlocking(() => Entity.SetPosition(v));
                OnPropertyChanged();
            }
        }

        public virtual bool CanStop => Entity.Physics?.Enabled ?? false;

        public virtual bool CanDelete => true;

        public virtual string GetSortedName(EntityTreeViewModel.SortEnum sort)
        {
            switch (sort)
            {
                case EntityTreeViewModel.SortEnum.Name:
                    return Name;
                case EntityTreeViewModel.SortEnum.Size:
                    return $"{Name} ({Entity.WorldVolume.Radius * 2:N}m)";
                case EntityTreeViewModel.SortEnum.Speed:
                    return $"{Name} ({Entity.Physics?.LinearVelocity.Length() ?? 0:N}m/s)";
                case EntityTreeViewModel.SortEnum.BlockCount:
                    if (Entity is MyCubeGrid grid)
                        return $"{Name} ({grid.BlocksCount} blocks)";

                    return Name;
                case EntityTreeViewModel.SortEnum.DistFromCenter:
                    return $"{Name} ({Entity.GetPosition().Length():N}m)";
                case EntityTreeViewModel.SortEnum.Owner:
                    if (Entity is MyCubeGrid g)
                        return $"{Name} ({g.GetGridOwnerName()})";

                    return Name;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }

        public virtual int CompareToSort(EntityViewModel other, EntityTreeViewModel.SortEnum sort)
        {
            if (other == null)
                return -1;

            switch (sort)
            {
                case EntityTreeViewModel.SortEnum.Name:
                    if (Name == null)
                    {
                        if (other.Name == null)
                            return 0;

                        return 1;
                    }

                    if (other.Name == null)
                        return -1;

                    return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
                case EntityTreeViewModel.SortEnum.Size:
                    return Entity.WorldVolume.Radius.CompareTo(other.Entity.WorldVolume.Radius);
                case EntityTreeViewModel.SortEnum.Speed:
                    if (Entity.Physics == null)
                    {
                        if (other.Entity.Physics == null)
                            return 0;

                        return -1;
                    }

                    if (other.Entity.Physics == null)
                        return 1;

                    return Entity.Physics.LinearVelocity.LengthSquared().CompareTo(other.Entity.Physics.LinearVelocity.LengthSquared());
                case EntityTreeViewModel.SortEnum.BlockCount:
                {
                    if (Entity is MyCubeGrid ga && other.Entity is MyCubeGrid gb)
                        return ga.BlocksCount.CompareTo(gb.BlocksCount);

                    goto case EntityTreeViewModel.SortEnum.Name;
                }
                case EntityTreeViewModel.SortEnum.DistFromCenter:
                    return Entity.GetPosition().LengthSquared().CompareTo(other.Entity.GetPosition().LengthSquared());
                case EntityTreeViewModel.SortEnum.Owner:
                {
                    if (Entity is MyCubeGrid ga && other.Entity is MyCubeGrid gb)
                        return string.Compare(ga.GetGridOwnerName(), gb.GetGridOwnerName(), StringComparison.InvariantCultureIgnoreCase);

                    goto case EntityTreeViewModel.SortEnum.Name;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }

        public virtual void Delete()
        {
            Entity.Close();
        }

        public class Comparer : IComparer<EntityViewModel>
        {
            private readonly EntityTreeViewModel.SortEnum _sort;

            public Comparer(EntityTreeViewModel.SortEnum sort)
            {
                _sort = sort;
            }

            public int Compare(EntityViewModel x, EntityViewModel y)
            {
                return x.CompareToSort(y, _sort);
            }
        }
    }
}