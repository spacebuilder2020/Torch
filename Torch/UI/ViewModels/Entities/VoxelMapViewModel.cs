using Sandbox.Game.Entities;
using Torch.Collections;


namespace Torch.UI.ViewModels.Entities
{
    public class VoxelMapViewModel : EntityViewModel
    {
        public VoxelMapViewModel(MyVoxelBase e, EntityTreeViewModel tree) : base(e, tree) { }

        public VoxelMapViewModel() { }

        private MyVoxelBase Voxel => (MyVoxelBase)Entity;

        public override string Name => string.IsNullOrEmpty(Voxel.StorageName) ? "UnnamedProcedural" : Voxel.StorageName;

        public override bool CanStop => false;

        public MtObservableList<GridViewModel> AttachedGrids { get; } = new MtObservableList<GridViewModel>();
    }
}