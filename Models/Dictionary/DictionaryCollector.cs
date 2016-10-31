using System;
using System.Collections.Generic;
using System.IO;



namespace GUIapp.Models.Dictionary
{
    class DictionaryCollector
    {
        public string path { get; set; }
        public Dictionary<string, double> lexicon = new Dictionary<string, double>();
        public Dictionary<string, int> result = new Dictionary<string, int>();

        private Dictionary<string, int> grammMap = new Dictionary<string, int>();
        private int emotion;

        public DictionaryCollector(string path)
        {
            this.path = path;
        }

        public Dictionary<string, int> collectDictionary()
        {
            using (var stream = new StreamReader(path))
            {
                string str = "";
                while ((str = stream.ReadLine()) != null)
                {
                    char[] temp = { ';' };
                    var parts = str.Split(temp, StringSplitOptions.RemoveEmptyEntries);
                    int tone;
                    if (parts.Length < 2 || !int.TryParse(parts[1], out tone))
                        continue;
                    parse(parts[0], tone);
                }
            }
            normalize(grammMap);
            return result;
        }

        private int getMax(Dictionary<string, int>.ValueCollection entries)
        {
            int max = int.MinValue;
            foreach (int val in entries)
            {
                if (val > max)
                    max = val;
            }
            return max;
        }

        private int getMin(Dictionary<string, int>.ValueCollection entries)
        {
            int min = int.MinValue;
            foreach (int val in entries)
            {
                if (val < min)
                    min = val;
            }
            return min;
        }

        private void normalize(Dictionary<string, int> oldLexicon)
        {
            int max = getMax(oldLexicon.Values);
            int min = getMin(oldLexicon.Values);

            foreach (var entry in oldLexicon)
            {
                if (entry.Value > 0)
                {
                    var temp = entry.Value * 1.0 / max;
                    if (temp > 0.3)
                        result.Add(entry.Key, 1);
                }
                else if (entry.Value < 0)
                {
                    var temp = entry.Value * -1.0 / min;
                    if (temp < -0.3)
                        result.Add(entry.Key, -1);
                }
            }
        }


        private void addGramm(string gramm)
        {
            if (grammMap.ContainsKey(gramm))
            {
                grammMap[gramm] = grammMap[gramm] + emotion;
            }
            else
            {
                grammMap.Add(gramm, emotion);
            }
        }

        public void parse(string inputString, int emotion)
        {
            var lemmatizer = new Lemmatizer();
            this.emotion = emotion;
            StringReader input = new StringReader(inputString);
            var vector = Lemmatizer.LemmatizeCurrentText(inputString);
            string prevWord = null;
            foreach (var str in vector)
            {
                addGramm(str);
                if (prevWord != null)
                {
                    addGramm(prevWord + " " + str);
                }
                prevWord = str;
            }
        }
        
    }
}
