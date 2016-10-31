using System;
using System.Collections.Generic;
using System.Linq;

namespace GUIapp.Models
{
    class ObjectInfo
    {
        public string ObjectName { get; set; }
        public int ListIndex { get; set; }
        public bool HasAt, HasHashTag;
        public List<string> _textSynonyms = new List<string>();

        public ObjectInfo(string objectName, int listIndex)
        {
            ObjectName = objectName;
            ListIndex = listIndex;
        }
    }

    class TextExtractor
    {
        public List<List<string>> _objects;

        private List<string> _ats = new List<string>();
        private List<string> _hashTags = new List<string>();
        private List<ObjectInfo> _textObjects = new List<ObjectInfo>();

        public TextExtractor(List<List<string>> objects)
        {
            _objects = objects;
        }

        private void refresh()
        {
            _ats.Clear();
            _hashTags.Clear();
            _textObjects.Clear();
        }

        private void AnalyzeText(string text)
        {
            var lemtext = Lemmatizer.LemmatizeCurrentText(text);
            for (int i = 0; i < _objects.Count; i++)
            {
                var list = _objects[i];
                ObjectInfo obj = null;
                foreach (var synonym in list)
                {
                    var lowsyn = synonym.ToLower();
                    if (lemtext.Contains(lowsyn))
                    {
                        if (obj == null)
                            obj = new ObjectInfo(list[0], i);
                        obj._textSynonyms.Add(synonym);
                    }
                    if (text.Contains(string.Concat("@", synonym)) || (synonym.Contains("@") && text.Contains(synonym)) 
                        || text.Contains(string.Concat("@", lowsyn)) || (synonym.Contains("@") && text.Contains(lowsyn)))
                    {
                        if (obj == null)
                            obj = new ObjectInfo(list[0], i);
                        obj.HasAt = true;
                        _ats.Add(synonym.Contains("@") ? synonym : string.Concat("@", synonym));
                    }
                    if (text.Contains(string.Concat("#", synonym)) || (synonym.Contains("#") && text.Contains(synonym))
                        || text.Contains(string.Concat("#", lowsyn)) || (synonym.Contains("#") && text.Contains(lowsyn)))
                    {
                        if (obj == null)
                            obj = new ObjectInfo(list[0], i);
                        obj.HasHashTag = true;
                        _hashTags.Add(synonym.Contains("#") ? synonym : string.Concat("#", synonym));
                    }
                }
                if (obj != null)
                    _textObjects.Add(obj);
            }
        }

        public List<ObjectInfo> getSentenceObjects(string sentence)
        {
            var result = new List<ObjectInfo>();

            foreach (var obj in _textObjects)
            {
                foreach (var synonym in obj._textSynonyms)
                {
                    if (Lemmatizer.LemmatizeCurrentText(sentence).Contains(synonym.ToLower()))
                        result.Add(obj);
                }
            }

            return result;
        }

        public Dictionary<string, string> ExtractTextParts(string text)
        {
            var result = new Dictionary<string, string>();
            refresh();
            AnalyzeText(text);
            char[] sentenceSeparators = { '.', '!', '?', '.' };
            char[] sentencePartSeparators = { ':', ',', ';' };

            //если в тексте нет упоминания объектов
            if (!_textObjects.Any())
            {
                return null;
            }

            var sentences = text.Split(sentenceSeparators, StringSplitOptions.RemoveEmptyEntries);

            //если в тексте упомянут только один объект
            if (_textObjects.Count() == 1)
            {
                //обработка @ и #
                if (_ats.Any() || _hashTags.Any())
                {
                    result.Add(_textObjects.First().ObjectName, text);
                    return result;
                }
                //общий случай
                
                foreach (var sent in sentences)
                {
                    foreach (var synonym in _textObjects.First()._textSynonyms)
                    {
                        if (Lemmatizer.LemmatizeCurrentText(sent).Contains(synonym.ToLower()))
                        {
                            var index = text.IndexOf(sent);
                            if (synonym == "билайн")
                            {
                                int a = 2;
                                a = a - result.Count;

                            }
                            result.Add(_textObjects.First().ObjectName, text.Substring(index));
                            return result;
                        }
                    }   
                }

                return result;
            }

            //если в тексте несколько объектов - берем части предложений по возможности
            ObjectInfo curObject = null;
            foreach (var str in sentences)
            {
                var list = getSentenceObjects(str);
                if (!list.Any())
                {
                    if (result.Any()) //если в предложении явно не указан объект, предложение конкатенируется к предыдущему для указанных в нем объектов
                    {
                        var last = result.Last();
                        for (int i = result.Count - 1; i >= 0; i--)
                        {
                            if (result.ElementAt(i).Value == last.Value)
                                result[result.ElementAt(i).Key] = string.Concat(result[result.ElementAt(i).Key], " ", str);
                            else
                                break;
                        }
                    }
                    else
                        continue;
                }
                else
                {
                    if (list.Count == 1)
                    {
                        curObject = list.First();
                        if (result.ContainsKey(curObject.ObjectName))
                            result[curObject.ObjectName] = string.Concat(result[curObject.ObjectName], " ", str);
                        else
                            result.Add(curObject.ObjectName, str);
                    }
                    else
                    {
                        var partsList = new List<string>(str.Split(sentencePartSeparators));
                        for (int i = 0; i < partsList.Count(); i++)
                        {
                            var dash = " - ";
                            if (partsList[i].Contains(dash))
                            {
                                partsList.Add(partsList[i].Substring(0, partsList[i].IndexOf(dash)));
                                partsList.Add(partsList[i].Substring(partsList[i].IndexOf(dash) + dash.Length));
                                partsList.RemoveAt(i);
                            }
                        }
                        foreach (var part in partsList)
                        {
                            var objs = getSentenceObjects(part);
                            foreach (var obj in objs)
                            {
                                if (result.ContainsKey(obj.ObjectName))
                                    result[obj.ObjectName] = string.Concat(result[obj.ObjectName], " ", part);
                                else
                                    result.Add(obj.ObjectName, part);
                            }
                        }
                    }
                }
            }
            return result;
        }

    }
}
