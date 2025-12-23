public interface IKeyValueStore
{
    bool Set(string key, string value);
    string? Get(string key);
    bool Delete(string key);
    KeyValuePair<string, string>[] List();
}