using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALLib.File;

namespace DALLib
{
    public class StringProcessor
    {

        public Dictionary<string, string> ReplacementDictionary = new Dictionary<string, string>();

        public void Load(string text, bool append = false)
        {
            if (!append)
                ReplacementDictionary.Clear();

            foreach (string line in text.Replace("\r", "").Split('\n'))
            {
                var split = line.Split('=');
                if (split.Length == 2 && !ReplacementDictionary.ContainsKey(split[1]))
                    ReplacementDictionary.Add(split[1], split[0]);
            }
        }

        public string Process(string text)
        {
            // Apply replacements
            foreach (var pair in ReplacementDictionary)
                text = text.Replace(pair.Key, pair.Value);

            return text;
        }
        public string ProcessReverse(string text)
        {
            // Apply replacements
            foreach (var pair in ReplacementDictionary)
                text = text.Replace(pair.Value, pair.Key);

            return text;
        }

    }
}
