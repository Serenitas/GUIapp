using GUIapp.Models.Dictionary;
using System;
using System.Collections.Generic;
using System.IO;


/*
Этот класс - часть моей дипломной работы "Разработка программы определения тональности сообщения относительно описываемого в нем объекта"
Работа класса заключается в получении от других классов исходных данных, их предобработка с помощью классов-предобработчиков и дальнейшая классификация текстов по тональности
словарным методом или методами машинного обучения.
*/



namespace GUIapp.Models
{
    class Classifier
    {
        public const int DictionaryMode = 0, MachineLearningMode = 1;

        private DictionaryCollector _collector = null;
        private FileHandler _fileHandler;
        private int _mode;
        public string Method { get; set; }
        public string TrainFilePath { get; set; }

        public Classifier(FileHandler fileHandler, int mode)
        {
            _fileHandler = fileHandler;
            _mode = mode;
        }

        public Classifier(DictionaryCollector collector, FileHandler fileHandler)
        {
            _collector = collector;
            _fileHandler = fileHandler;
            _mode = DictionaryMode;
        }

        public void Classify(string outputPath)
        {
            var dataList = _fileHandler.DataList;

            #region DictionaryMode 
            if (_mode == DictionaryMode)  //словарный метод определения тональности текста
            {
                var classifier = new DictionaryClassifier();
                if (_collector != null)
                    classifier.SpecificLexicon = _collector.collectDictionary();  //сбор дополнительного словаря по заданной пользователем выборке
                if (_fileHandler.Objects == null)
                {
                    //определение общей тональности текста
                    using (var stream = new StreamWriter(outputPath))
                    {
                        stream.WriteLine("text;tone"); //заголовок таблицы
                        foreach (var text in dataList)
                        {
                            var tone = classifier.Classify(text);
                            stream.WriteLine(string.Concat(text, ";", tone));
                        }
                    }
                }
                else
                {
                    //определение объектно-ориентированной тональности
                    var extractor = new TextExtractor(_fileHandler.Objects);
                    using (var stream = new StreamWriter(outputPath))
                    {
                        stream.Write("text"); //заголовок таблицы
                        foreach (var obj in _fileHandler.Objects)
                        {
                            stream.Write(string.Concat(";", obj[0]));
                        }
                        stream.WriteLine();
                        foreach (var text in dataList)
                        {
                            var list = extractor.ExtractTextParts(text);
                            stream.Write(text);
                            foreach (var entry in _fileHandler.Objects)
                            {
                                int tone = 0;
                                if (list != null && list.ContainsKey(entry[0]))
                                    tone = classifier.Classify(list[entry[0]]);
                                stream.Write(string.Concat(";", tone));
                            }
                            stream.WriteLine();
                        }
                    }
                }
            }
            #endregion
            #region MachineLearningMode
            if (_mode == MachineLearningMode)
            {
                var classifier = new MachineLearningClassifier(Method);
                string testSourcePath = _fileHandler.DataFilePath;
                string testPath = Path.Combine(Environment.CurrentDirectory, "test.csv");
                string trainPath = Path.Combine(Environment.CurrentDirectory, "train.csv");

                var texts = new List<string>();
                var tones = new List<int>();

                using (var reader = new StreamReader(TrainFilePath))  //чтение обучающей выборки
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        char[] temp = { ';' };
                        var parts = str.Split(temp, StringSplitOptions.RemoveEmptyEntries);
                        int tone;
                        if (parts.Length < 2 || !int.TryParse(parts[1], out tone))
                            continue;
                        texts.Add(parts[0]);
                        tones.Add(tone);
                    }
                }
                
                var vectorizer = new Vectorizer(texts);

                using (var writer = new StreamWriter(trainPath)) //запись таблицы с обучающей выборкой, векторизованной методом bag-of-words
                {
                    for (int i = 0; i < vectorizer.DictSize; i++) 
                    {
                        writer.Write(string.Concat(i, "w", ";"));
                    }
                    writer.WriteLine("tone");
                    for (int i = 0; i < texts.Count; i++)
                    {
                        var vector = vectorizer.GetVector(texts[i]);
                        foreach (var num in vector)
                        {
                            writer.Write(string.Concat(num, ";"));
                        }
                        writer.WriteLine(tones[i]);
                    }
                }
                texts.Clear();

                if (_fileHandler.Objects == null)
                {
                    //классификация текстов по общей тональности
                    using (var reader = new StreamReader(testSourcePath))
                    {
                        string str;
                        while ((str = reader.ReadLine()) != null)
                        {
                            texts.Add(str);
                        }
                    }
                    using (var writer = new StreamWriter(testPath))
                    {
                        for (int i = 0; i < texts.Count; i++)
                        {
                            var vector = vectorizer.GetVector(texts[i]);
                            writer.Write(vector[0]);
                            for(int j = 1; j < vector.Count; j++)
                            {
                                writer.Write(string.Concat(";", vector[j]));
                            }
                            writer.WriteLine();
                        }
                    }
                    classifier.Classify(trainPath, testPath, outputPath, Method, false);
                }
                else
                {
                    //объектная тональность
                    using (var reader = new StreamReader(testSourcePath))
                    {
                        string str;
                        while ((str = reader.ReadLine()) != null)
                        {
                            texts.Add(str);
                        }
                    }
                    using (var writer = new StreamWriter(testPath))
                    {
                        writer.Write("textnum;objectname");
                        for (int i = 0; i < vectorizer.DictSize; i++)
                        {
                            writer.Write(string.Concat(";", i, "w"));
                        }
                        writer.WriteLine();
                        for (int i = 0; i < texts.Count; i++)
                        {
                            var extractor = new TextExtractor(_fileHandler.Objects);
                            var list = extractor.ExtractTextParts(texts[i]); //извлечение из текста мультимножества его частей, каждая из которых описывает объект из заданного списка
                            if (list == null)
                                continue;
                            foreach (var part in list)
                            {
                                writer.Write(string.Concat(i, ";", part.Key));
                                var vector = vectorizer.GetVector(texts[i]); 
                                foreach (var num in vector)
                                {
                                    writer.Write(string.Concat(";", num));
                                }
                                writer.WriteLine();
                            }                            
                        }
                    }
                    classifier.Classify(trainPath, testPath, outputPath, Method, true);
                }
            }
            #endregion
        }
    }
}
