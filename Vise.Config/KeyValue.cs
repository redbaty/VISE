using System.Collections.Generic;
using System.Linq;

namespace Vise.Config
{
    internal class KeyValue
    {
        public string Key { get; set; }

        public List<string> Value { get; set; } = new List<string>();

        public KeyValue(string line)
        {
            Key = line.Split('=')[0];

            var valueString = line.Split('=')[1];

            if (!string.IsNullOrEmpty(valueString))
                Value = valueString.Split(':').ToList();
        }

        public void AppendValue(string value)
        {
            Value.Add(value);
        }

        public override string ToString()
        {
            var returnString = $"{Key}=";
            for (var i = 0; i < Value.Count; i++)
            {
                if (i > 0)
                    returnString += $":{Value[i]}";
                else
                    returnString += Value[i];
            }

            return returnString;
        }
    }
}