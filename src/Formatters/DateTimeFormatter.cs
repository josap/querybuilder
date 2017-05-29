using System;

namespace SqlKata.Formatters
{
    public class DateTimeFormatter : Formatter
    {
        public string DateTimeFormat { get; set; }
        public override string Format(object value)
        {
            var val = (DateTime)value;
            return val.ToString(DateTimeFormat);
        }
    }
}