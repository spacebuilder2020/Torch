using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using NLog;
using NLog.Targets;
using Torch.Extensions;

namespace Torch.UI
{
    /// <summary>
    ///     NLog target that writes to a <see cref="FlowDocument" />.
    /// </summary>
    [Target("flowDocument")]
    public sealed class FlowDocumentTarget : TargetWithLayout
    {
        private static readonly Dictionary<LogLevel, SolidColorBrush> LogLevelColors = new Dictionary<LogLevel, SolidColorBrush>
        {
            [LogLevel.Trace] = new SolidColorBrush(Colors.DimGray),
            [LogLevel.Debug] = new SolidColorBrush(Colors.DarkGray),
            [LogLevel.Info] = new SolidColorBrush(Colors.White),
            [LogLevel.Warn] = new SolidColorBrush(Colors.Magenta),
            [LogLevel.Error] = new SolidColorBrush(Colors.Yellow),
            [LogLevel.Fatal] = new SolidColorBrush(Colors.Red),
        };

        private readonly int _maxLines = 500;
        private readonly Paragraph _paragraph = new Paragraph();

        public FlowDocumentTarget()
        {
            Document.Blocks.Add(_paragraph);
        }

        public FlowDocument Document { get; } = new FlowDocument {Background = new SolidColorBrush(Colors.Black)};

        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            Document.Dispatcher.BeginInvoke(() =>
            {
                var message = $"{Layout.Render(logEvent)}\n";
                _paragraph.Inlines.Add(new Run(message) {Foreground = LogLevelColors[logEvent.Level]});

                // A massive paragraph slows the UI down
                if (_paragraph.Inlines.Count > _maxLines)
                    _paragraph.Inlines.Remove(_paragraph.Inlines.FirstInline);
            });
        }
    }
}