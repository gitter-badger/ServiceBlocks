namespace ServiceBlocks.Messaging.Common
{
    public interface IVersionFilter<TValue> where TValue : class
    {
        bool TryTakeNewerVersion(TValue currentValue, TValue newValue, out TValue latestValue);
    }
}