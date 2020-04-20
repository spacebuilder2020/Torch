using Sandbox.Game.Entities;

namespace Torch.UI.ViewModels.Entities
{
    public class FloatingObjectViewModel : EntityViewModel
    {
        public FloatingObjectViewModel(MyFloatingObject floating, EntityTreeViewModel tree) : base(floating, tree) { }
        private MyFloatingObject Floating => (MyFloatingObject)Entity;

        public override string Name => $"{base.Name} ({Floating.Amount})";
    }
}