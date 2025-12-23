public interface IKeyValueStore
{
    void Set(string key, string value);
    string? Get(string key);
    void Delete(string key);
    KeyValuePair<string, string>[] List();
}