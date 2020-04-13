using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox.Game.Entities;
using Torch.Collections;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace Torch.Server.ViewModels.Entities
{
    public class VoxelMapViewModel : EntityViewModel
    {
        public VoxelMapViewModel(MyVoxelBase e, EntityTreeViewModel tree) : base(e, tree) { }

        public VoxelMapViewModel() { }

        private MyVoxelBase Voxel => (MyVoxelBase)Entity;

        public override string Name => string.IsNullOrEmpty(Voxel.StorageName) ? "UnnamedProcedural" : Voxel.StorageName;

        public override bool CanStop => false;

        public MtObservableList<GridViewModel> AttachedGrids { get; } = new MtObservableList<GridViewModel>();

        public async Task UpdateAttachedGrids()
        {
            //TODO: fix
            return;

            AttachedGrids.Clear();
            var box = Entity.WorldAABB;
            var entities = new List<MyEntity>();
            await TorchBase.Instance.InvokeAsync(() => MyEntities.GetTopMostEntitiesInBox(ref box, entities)).ConfigureAwait(false);
            foreach (var entity in entities.Where(e => e is IMyCubeGrid))
            {
                if (Tree.Grids.TryGetValue(entity.EntityId, out var gridModel))
                {
                    gridModel = new GridViewModel((MyCubeGrid)entity, Tree);
                    Tree.Grids.Add(entity.EntityId, gridModel);
                }

                AttachedGrids.Add(gridModel);
            }
        }
    }
}