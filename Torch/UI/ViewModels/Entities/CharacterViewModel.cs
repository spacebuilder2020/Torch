using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;

namespace Torch.UI.ViewModels.Entities
{
    public class CharacterViewModel : EntityViewModel
    {
        private readonly MyCharacter _character;

        public CharacterViewModel(MyCharacter character, EntityTreeViewModel tree) : base(character, tree)
        {
            _character = character;
            character.ControllerInfo.ControlAcquired += ControllerInfo_ControlAcquired;
            character.ControllerInfo.ControlReleased += ControllerInfo_ControlAcquired;
        }

        public CharacterViewModel() { }

        public override bool CanDelete => _character.ControllerInfo?.Controller?.Player == null;

        private void ControllerInfo_ControlAcquired(MyEntityController obj)
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(CanDelete));
        }
    }
}