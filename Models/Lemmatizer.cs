using System;
using System.Collections.Generic;
using System.Text;



namespace GUIapp.Models
{
    class Lemmatizer
    {
        private static List<string> _vector = new List<string>();

        public static List<string> Stopwords { get; set; }

        private static System.Diagnostics.Process proc = null;

        public static List<string> LemmatizeCurrentText(string currentText)
        {
            var curdir = Environment.CurrentDirectory;
            _vector.Clear();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = curdir + "\\mystem.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "-n -d -l -e cp1251",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var bytes = Encoding.Unicode.GetBytes(currentText + "\nENDOFSTREAM");
            var newtext = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding("Windows-1251"), bytes);
            string strn = Encoding.GetEncoding("Windows-1251").GetString(newtext);


            if (proc == null) proc = System.Diagnostics.Process.Start(startInfo); //запуск лемматизатора
            var input = proc.StandardInput;
            input.WriteLine(strn);
            var stream = proc.StandardOutput;
            string str = stream.ReadLine();
            while (!str.Contains("ENDOFSTREAM"))
            {
                str = str.Replace("?", "").ToLower();
                bytes = Encoding.GetEncoding("Windows-1251").GetBytes(str);
                newtext = Encoding.Convert(Encoding.GetEncoding("Windows-1251"), Encoding.Unicode, bytes);
                strn = Encoding.Unicode.GetString(newtext);
                _vector.Add(strn); //вектор слов
                str = stream.ReadLine();
            }

            return _vector;
        }

        
    }
}
