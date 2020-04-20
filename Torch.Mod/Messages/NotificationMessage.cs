using System;
using ProtoBuf;
using Sandbox.ModAPI;

namespace Torch.Messages
{
    [ProtoContract]
    public class NotificationMessage : MessageBase
    {
        [ProtoMember(203)]
        public int DisappearTimeMs;

        [ProtoMember(202)]
        public string Font;

        [ProtoMember(201)]
        public string Message;

        public NotificationMessage() { }

        public NotificationMessage(string message, int disappearTimeMs, string font)
        {
            Message = message;
            DisappearTimeMs = disappearTimeMs;
            Font = font;
        }

        public override void ProcessClient()
        {
            MyAPIGateway.Utilities.ShowNotification(Message, DisappearTimeMs, Font);
        }

        public override void ProcessServer()
        {
            throw new Exception();
        }
    }
}