namespace TimeBasedKVStore.Interfaces
{
    public interface ISerializer<T>
    {
        void Serialize(T value);

        T Deserialize();
    }
}