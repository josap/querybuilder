namespace SqlKata.Execution
{
    public interface ICacheProvider
    {
        void Store(string key, object value);
        void Get(string key);
        void Remove(string key);
    }
}