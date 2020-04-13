using Sandbox.Game.Entities;
using Torch.Server.ViewModels.Entities;

namespace Torch.Server.ViewModels
{
    public class FloatingObjectViewModel : EntityViewModel
    {
        public FloatingObjectViewModel(MyFloatingObject floating, EntityTreeViewModel tree) : base(floating, tree) { }
        private MyFloatingObject Floating => (MyFloatingObject)Entity;

        public override string Name => $"{base.Name} ({Floating.Amount})";
    }
}