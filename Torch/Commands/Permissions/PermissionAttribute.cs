using System;
using VRage.Game.ModAPI;

namespace Torch.Commands.Permissions
{
    public class PermissionAttribute : Attribute
    {
        public PermissionAttribute(MyPromoteLevel promoteLevel)
        {
            PromoteLevel = promoteLevel;
        }

        public MyPromoteLevel PromoteLevel { get; }
    }
}