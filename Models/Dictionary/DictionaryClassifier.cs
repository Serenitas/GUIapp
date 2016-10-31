using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GUIapp.Models.Dictionary
{
    class DictionaryClassifier
    {
        private Dictionary<string, int> _sentilex = new Dictionary<string, int>();
        private Dictionary<string, int> _specificLexicon = new Dictionary<string, int>();
        private string[] _negations = { "не", "ни", "перестать", "прекратить" };
        private string[] _gains = { "очень", "вполне", "особо", "слишком" };
        private List<string> _stopwords = new List<string>();

        public Dictionary<string, int> SpecificLexicon
        {
            get
            {
                return _specificLexicon;
            }

            set
            {
                _specificLexicon = value;
            }
        }

        public DictionaryClassifier()
        {
            var sentiLexPath = string.Concat(Environment.CurrentDirectory, "//sentilex.txt");
            ReadSentiLex(sentiLexPath);
        }

        public DictionaryClassifier(string specificPath)
        {
            var sentiLexPath = string.Concat(Environment.CurrentDirectory, "//sentilex.txt");
            ReadSentiLex(sentiLexPath);
            ReadSpecificLexicon(specificPath);
        }

        private void ReadSentiLex(string fullfilepath)
        {
            using (var stream = new StreamReader(fullfilepath))
            {
                var str = "";
                var prevWord = "";
                while ((str = stream.ReadLine()) != null)
                {
                    var word = str.Split(',')[0];
                    if (word.Contains(' '))
                        continue;
                    if (word == prevWord)
                    {
                        if (_sentilex.ContainsKey(word))
                        {
                            var tones = new List<int>();
                            var curTone = _sentilex[word];
                            tones.Add(curTone);
                            while (word == prevWord)
                            {
                                switch (str.Split(',')[3].Trim())
                                {
                                    case ("negative"):
                                        tones.Add(-1);
                                        break;
                                    case ("positive"):
                                        tones.Add(1);
                                        break;
                                    default:
                                        break;
                                }
                                str = stream.ReadLine();
                                if (str == null || str.Split(',')[0].Contains(' '))
                                {
                                    var tone1 = tones.Sum() / (double)tones.Count;
                                    if (tone1 < -0.3)
                                        _sentilex[word] = -1;
                                    else if (tone1 > 0.3)
                                        _sentilex[word] = 1;
                                    else
                                        _sentilex[word] = 0;
                                    if (str == null)
                                        break;
                                }
                                word = str.Split(',')[0];
                            }
                            var tone = tones.Sum() / (double)tones.Count;
                            if (tone < -0.3)
                                _sentilex[prevWord] = -1;
                            else if (tone > 0.3)
                                _sentilex[prevWord] = 1;
                            else
                                _sentilex[prevWord] = 0;
                        }
                    }
                    switch (str.Split(',')[3].Trim())
                    {
                        case ("negative"):
                            _sentilex.Add(word, -1);
                            break;
                        case ("positive"):
                            _sentilex.Add(word, 1);
                            break;
                        default:
                            break;
                    }
                    prevWord = word;
                }
            }
        }

        private void ReadSpecificLexicon(string path)
        {
            try
            {
                using (var stream = new StreamReader(path))
                {
                    string word;
                    while ((word = stream.ReadLine()) != null)
                    {
                        int tone = int.Parse(stream.ReadLine());
                        _specificLexicon.Add(word, tone);
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void ReadStopWords(string path)
        {
            try
            {
                using (var stream = new StreamReader(path))
                {
                    string word;
                    while ((word = stream.ReadLine()) != null)
                    {
                        _stopwords.Add(word);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public int Classify(string text)
        {
            Lemmatizer.Stopwords = _stopwords;
            var vector = Lemmatizer.LemmatizeCurrentText(text);
            int sign = 1, tone = 0, toneCount = 0;
            for (int i = 0; i < vector.Count; i++)
            {
                if (_sentilex.ContainsKey(vector[i]) || _specificLexicon.ContainsKey(vector[i]))
                {
                    int basictone = (_specificLexicon.ContainsKey(vector[i])) ? _specificLexicon[vector[i]] : _sentilex[vector[i]];
                    if (i - 1 > 0)
                    {
                        if (_negations.Contains(vector[i - 1]))
                            sign = -1;
                        if (_gains.Contains(vector[i - 1]))
                            if (i - 2 > 0 && _negations.Contains(vector[i - 2]))
                                sign = -1;
                    }
                    tone += basictone * sign;
                    toneCount++;
                }
            }
            if (toneCount > 0)
            {
                var doubletone = tone / (toneCount + 0.0);
                if (doubletone > 0.3)
                    return 1;
                else if (doubletone < -0.3)
                    return -1;
                else
                    return 0;
            }
            else
                return 0;
        }
    }
}
