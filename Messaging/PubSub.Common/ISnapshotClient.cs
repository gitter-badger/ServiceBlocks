using System;

namespace ServiceBlocks.Messaging.Common
{
    public interface ISnapshotClient
    {
        byte[] GetSnapshot(string topic);
    }

    public interface ISnapshotClient<T>
    {
        T GetAndParseSnapshot(string topic);
    }

    public abstract class SnapshotClient<T> : ISnapshotClient, ISnapshotClient<T>
    {
        private readonly Func<byte[], T> _deserializer;

        protected SnapshotClient(Func<byte[], T> deserializer)
        {
            _deserializer = deserializer;
        }

        public abstract byte[] GetSnapshot(string topic);

        T ISnapshotClient<T>.GetAndParseSnapshot(string topic)
        {
            return _deserializer(GetSnapshot(topic));
        }
    }
}