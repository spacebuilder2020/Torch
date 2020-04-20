using ProtoBuf;
using Sandbox.ModAPI;

namespace Torch.Messages
{
    [ProtoContract]
    public class JoinServerMessage : MessageBase
    {
        [ProtoMember(202)]
        public string Address;

        [ProtoMember(201)]
        public int Delay;

        private JoinServerMessage() { }

        public JoinServerMessage(string address)
        {
            Address = address;
        }

        public JoinServerMessage(string address, int delay)
        {
            Address = address;
            Delay = delay;
        }

        public override void ProcessClient()
        {
            if (TorchModCore.Debug)
            {
                MyAPIGateway.Utilities.ShowMessage("Torch", $"Joining server {Address} with delay {Delay}");
            }

            if (Delay <= 0)
            {
                MyAPIGateway.Multiplayer.JoinServer(Address);
                return;
            }

            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyAPIGateway.Parallel.Sleep(Delay);
                MyAPIGateway.Multiplayer.JoinServer(Address);
            });
        }

        public override void ProcessServer() { }
    }
}