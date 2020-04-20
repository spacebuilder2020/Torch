using System;
using System.Windows.Threading;

namespace Torch.Extensions
{
    public static class DispatcherExtensions
    {
        public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return dispatcher.BeginInvoke(priority, action);
        }
    }
}