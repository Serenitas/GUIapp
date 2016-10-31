using System;
using System.IO;
using System.Windows;

namespace GUIapp.Models
{
    class MachineLearningClassifier
    {
        private string _methodName;

        public MachineLearningClassifier(string methodName)
        {
            _methodName = methodName;
        }

        public void Classify(string trainPath, string testPath, string outPath, string method, bool isObject)
        {
            var scriptName = isObject ? "object_script.py" : "general_script.py";
            var python = Path.Combine(Environment.CurrentDirectory, "WinPython-32bit-2.7.10.3", "python-2.7.10", "python.exe");

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = python,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = scriptName + " " + trainPath + " " + testPath + " " + outPath + " " + method,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = System.Diagnostics.Process.Start(startInfo);

            var output = proc.StandardOutput;
            var error = proc.StandardError;

            proc.WaitForExit();

            var str = proc.StandardOutput.ReadToEnd();
            var str2 = proc.StandardError.ReadToEnd();
            if (str2 != "" && !str2.Contains("DataConversionWarning"))
            {
                MessageBox.Show(str2, "Ошибка выполнения скрипта Python", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
