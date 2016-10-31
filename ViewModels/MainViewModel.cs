using Caliburn.Micro;
using GUIapp.Models;
using GUIapp.Models.Dictionary;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace GUIapp.ViewModels
{
    class MainViewModel : Screen
    {
        private const string resultDirectoryName = "results";
        private const string resultName = "_result.csv";

        private IWindowManager _manager;
        private string _dataFilePath, _objectFilePath, _dictionaryDataFilePath, _trainFilePath;
        private bool _isMachineLearningEnabled = false, _isDictionaryEnabled = false, _isSentilex = false, _isNewDict = false;
        private bool _isSVM = false, _isRandomForest = false, _isLogisticRegression = false, _isStochasticGradient = false, _isBernoulliNB = false;

        #region properties
        public bool IsMachineLearningEnabled
        {
            get
            {
                return _isMachineLearningEnabled;
            }
            set
            {
                if (value == _isMachineLearningEnabled)
                    return;
                _isMachineLearningEnabled = value;
                NotifyOfPropertyChange(() => IsMachineLearningEnabled);
            }
        }

        public bool IsDictionaryEnabled
        {
            get
            {
                return _isDictionaryEnabled;
            }
            set
            {
                if (value == _isDictionaryEnabled)
                    return;
                _isDictionaryEnabled = value;
                NotifyOfPropertyChange(() => IsDictionaryEnabled);
            }
        }

        public bool IsSVM
        {
            get
            {
                return _isSVM;
            }
            set
            {
                if (value == _isSVM)
                    return;
                _isSVM = value;
                NotifyOfPropertyChange(() => IsSVM);
            }
        }

        public bool IsRandomForest
        {
            get
            {
                return _isRandomForest;
            }
            set
            {
                if (value == _isRandomForest)
                    return;
                _isRandomForest = value;
                NotifyOfPropertyChange(() => IsRandomForest);
            }
        }

        public bool IsLogisticRegression
        {
            get
            {
                return _isLogisticRegression;
            }
            set
            {
                if (value == _isLogisticRegression)
                    return;
                _isLogisticRegression = value;
                NotifyOfPropertyChange(() => IsLogisticRegression);
            }
        }

        public bool IsStochasticGradient
        {
            get
            {
                return _isStochasticGradient;
            }
            set
            {
                if (value == _isStochasticGradient)
                    return;
                _isStochasticGradient = value;
                NotifyOfPropertyChange(() => IsStochasticGradient);
            }
        }

        public bool IsBernoulliNB
        {
            get
            {
                return _isBernoulliNB;
            }
            set
            {
                if (value == _isBernoulliNB)
                    return;
                _isBernoulliNB = value;
                NotifyOfPropertyChange(() => IsBernoulliNB);
            }
        }

        public bool IsSentilex
        {
            get
            {
                return _isSentilex;
            }
            set
            {
                if (value == _isSentilex)
                    return;
                _isSentilex = value;
                NotifyOfPropertyChange(() => IsSentilex);
            }
        }

        public bool IsNewDict
        {
            get
            {
                return _isNewDict;
            }
            set
            {
                if (value == _isNewDict)
                    return;
                _isNewDict = value;
                NotifyOfPropertyChange(() => IsNewDict);
            }
        }

        public string DataFilePath
        {
            get
            {
                return _dataFilePath;
            }
            set
            {
                if (value == _dataFilePath)
                    return;
                _dataFilePath = value;
                NotifyOfPropertyChange(() => DataFilePath);
            }
        }

        public string ObjectFilePath
        {
            get
            {
                return _objectFilePath;
            }
            set
            {
                if (value == _objectFilePath)
                    return;
                _objectFilePath = value;
                NotifyOfPropertyChange(() => ObjectFilePath);
            }
        }

        public string DictionaryDataFilePath
        {
            get
            {
                return _dictionaryDataFilePath;
            }
            set
            {
                if (value == _dictionaryDataFilePath)
                    return;
                _dictionaryDataFilePath = value;
                NotifyOfPropertyChange(() => DictionaryDataFilePath);
            }
        }

        public string TrainFilePath
        {
            get
            {
                return _trainFilePath;
            }
            set
            {
                if (value == _trainFilePath)
                    return;
                _trainFilePath = value;
                NotifyOfPropertyChange(() => TrainFilePath);
            }
        }

        #endregion

        public MainViewModel(IWindowManager manager)
        {
            _manager = manager;

            DisplayName = "Классификатор тональности текста";
        }

        public void BrowseDataFile()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Файлы формата csv|*.csv";
            if (fd.ShowDialog() != null)
            {
                DataFilePath = fd.FileName;
            }
        }

        public void BrowseTrainFile()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Файлы формата csv|*.csv";
            if (fd.ShowDialog() != null)
            {
                TrainFilePath = fd.FileName;
            }
        }

        public void BrowseObjectFile()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Файлы формата csv|*.csv";
            if (fd.ShowDialog() != null)
            {
                ObjectFilePath = fd.FileName;
            }
        }

        public void BrowseDictionaryDataFile()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Файлы формата csv|*.csv";
            if (fd.ShowDialog() != null)
            {
                DictionaryDataFilePath = fd.FileName;
            }
        }

        public void EnableMachineLearning()
        {
            IsMachineLearningEnabled = true;
            IsDictionaryEnabled = false;
            IsSentilex = false;
            IsNewDict = false;
        }

        public void EnableDictionary()
        {
            IsMachineLearningEnabled = false;
            IsSVM = false;
            IsDictionaryEnabled = true;
        }

        public void Start()
        {
            if (DataFilePath == null || DataFilePath == "")
            {
                MessageBox.Show("Не был выбран файл с данными для классификации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                FileHandler fileHandler;
                if (ObjectFilePath != null && ObjectFilePath != "")
                {
                    fileHandler = new FileHandler(DataFilePath, ObjectFilePath);
                }
                else
                {
                    fileHandler = new FileHandler(DataFilePath);
                }

                var directory = Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, resultDirectoryName));
                var resultFileName = string.Concat(Path.GetFileNameWithoutExtension(DataFilePath), resultName); 
                var resultPath = Path.Combine(directory.FullName, resultFileName);

                if (IsDictionaryEnabled)
                {
                    //словарный метод классификации
                    Classifier classifier;
                    if (IsNewDict)
                    {
                        if (DictionaryDataFilePath == null || DictionaryDataFilePath == "")
                        {
                            MessageBox.Show("Не был выбран файл с размеченной выборкой для создания словаря", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var collector = new DictionaryCollector(DictionaryDataFilePath);
                        classifier = new Classifier(collector, fileHandler);
                    }
                    else
                    {
                        classifier = new Classifier(fileHandler, Classifier.DictionaryMode);
                    }
                    classifier.Classify(resultPath);
                    MessageBox.Show(string.Format("Работа программы успешно завершена! Результаты сохранены в файле {0} в подкаталоге {1}.", resultFileName, resultDirectoryName));
                }
                else if (IsMachineLearningEnabled)
                {
                    //машинное обучение
                    if (TrainFilePath == null || TrainFilePath == "")
                    {
                        MessageBox.Show("Не был выбран файл с обучающей выборкой", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var classifier = new Classifier(fileHandler, Classifier.MachineLearningMode);
                    if (IsSVM)
                    {
                        classifier.Method = "svm";
                    }
                    else if (IsStochasticGradient)
                    {
                        classifier.Method = "stochastic_gradient";
                    }
                    else if (IsRandomForest)
                    {
                        classifier.Method = "random_forest";
                    }
                    else if (IsLogisticRegression)
                    {
                        classifier.Method = "logistic_regression";
                    }
                    else if (IsBernoulliNB)
                    {
                        classifier.Method = "bernoulli_naive_bayes";
                    }
                    else
                    {
                        MessageBox.Show("Не был выбран способ классификации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    classifier.TrainFilePath = TrainFilePath;
                    classifier.Classify(resultPath);
                    MessageBox.Show(string.Format("Работа программы успешно завершена! Результаты сохранены в файле {0} в подкаталоге {1}.", resultFileName, resultDirectoryName));
                }
                else
                {
                    MessageBox.Show("Не был выбран способ классификации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
