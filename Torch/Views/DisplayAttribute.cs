using System;

namespace Torch.Views
{
    public class DisplayAttribute : Attribute
    {
        public string Description;
        public Type EditorType = null;
        public bool Enabled = true;
        public string GroupName;
        public string Name;
        public int Order;
        public bool ReadOnly = false;
        public string ToolTip;
        public bool Visible = true;

        public DisplayAttribute() { }

        public static implicit operator DisplayAttribute(System.ComponentModel.DataAnnotations.DisplayAttribute da)
        {
            if (da == null)
                return null;

            return new DisplayAttribute()
            {
                Name = da.GetName(),
                Description = da.GetDescription(),
                GroupName = da.GetGroupName(),
                Order = da.GetOrder() ?? 0
            };
        }
    }
}