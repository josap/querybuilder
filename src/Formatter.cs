namespace SqlKata
{
    public class Formatter
    {
        public virtual string Format(object value)
        {
            return value.ToString();
        }
    }
}