using System.Collections.Generic;
using System.IO;

namespace GUIapp.Models
{
    class FileHandler
    {
        public List<string> DataList { get; set; }
        public List<List<string>> Objects { get; set; }
        public string DataFilePath { get; set; }
        public string ObjectFilePath { get; set; }

        public FileHandler(string dataFilePath)
        {
            DataFilePath = dataFilePath;
            DataList = new List<string>();
            ReadDataFile(DataFilePath);
            Objects = null;
        }

        public FileHandler(string dataFilePath, string objectsFilePath)
        {
            DataFilePath = dataFilePath;
            ObjectFilePath = objectsFilePath;
            DataList = new List<string>();
            Objects = new List<List<string>>();
            ReadDataFile(DataFilePath);
            ReadObjectFile(objectsFilePath);
        }

        public List<string> ReadDataFile(string filepath)
        {
            using (var stream = new StreamReader(filepath))
            {
                string str = null;
                while ((str = stream.ReadLine()) != null)
                {
                    DataList.Add(str);
                }
            }

            return DataList;
        }

        public List<List<string>> ReadObjectFile(string filepath)
        {
            using (var stream = new StreamReader(filepath))
            {
                string str = null;
                while ((str = stream.ReadLine()) != null)
                {
                    Objects.Add(new List<string>(str.Split(';')));
                }
            }

            return Objects;
        }
    }
}
