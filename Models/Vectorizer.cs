using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIapp.Models
{
    class Vectorizer
    {
        private static string[] emoticons = { "(", ")", ":(", ":)", ";(", ";)", "xD", "^", ">", ":D", "D:", ");", "):", "(:", "(;", "T_", "Т_" };
        private List<string> _dict;
        public int DictSize
        {
            get
            {
                return _dict.Count;
            }
        }

        public Vectorizer(List<string> texts)
        {
            _dict = new List<string>();

            foreach (var text in texts)
            {
                var lemmatized = Lemmatizer.LemmatizeCurrentText(text);
                foreach (var word in lemmatized)
                    if (!_dict.Contains(word))
                        _dict.Add(word);
                foreach (var emoticon in emoticons)
                    if (text.Contains(emoticon) && !_dict.Contains(emoticon))
                        _dict.Add(emoticon);
            }
        }

        public List<int> GetVector(string text)
        {
            var result = new List<int>();
            var lemmatized = Lemmatizer.LemmatizeCurrentText(text);

            foreach (var word in _dict)
            {
                if (lemmatized.Contains(word))
                    result.Add(1);
                else
                    result.Add(0);
            }

            return result;
        }
    }
}
