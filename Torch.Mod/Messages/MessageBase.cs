using ProtoBuf;

namespace Torch.Messages
{
    #region Includes

    [ProtoInclude(1, typeof(DialogMessage))]
    [ProtoInclude(2, typeof(NotificationMessage))]
    [ProtoInclude(3, typeof(VoxelResetMessage))]
    [ProtoInclude(4, typeof(JoinServerMessage))]

    #endregion

    [ProtoContract]
    public abstract class MessageBase
    {
        internal byte[] CompressedData;
        internal ulong[] Ignore;

        [ProtoMember(101)]
        public ulong SenderId;

        internal ulong Target;

        //members below not serialized, they're just metadata about the intended target(s) of this message
        internal MessageTarget TargetType;

        public abstract void ProcessClient();
        public abstract void ProcessServer();
    }

    public enum MessageTarget
    {
        /// <summary>
        ///     Send to Target
        /// </summary>
        Single,

        /// <summary>
        ///     Send to Server
        /// </summary>
        Server,

        /// <summary>
        ///     Send to all Clients (only valid from server)
        /// </summary>
        AllClients,

        /// <summary>
        ///     Send to all except those steam ID listed in Ignore
        /// </summary>
        AllExcept,
    }
}