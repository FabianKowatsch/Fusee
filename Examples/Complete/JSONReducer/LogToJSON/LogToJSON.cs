using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace LogToJSON
{
    class LogToJSON
    {
        private static string pathToInput = "G:\\Projects\\Fusee\\Examples\\Complete\\JSONReducer\\LogToJSON\\logInput\\ladybug_front.log";
        private static string pathToOutput = "G:\\Projects\\Fusee\\Examples\\Complete\\JSONReducer\\input";
        static void Main(string[] args)
        {
            string logText = File.ReadAllText(pathToInput);
            var list = LogToList(logText);
            WriteListToFile(list);

        }
        private static List<Dictionary<string, string>> LogToList(string log)
        {

            var txtArray = log.Split('\n', '\r', ';', ' ');
            var keys = txtArray.Take(28);
            var values = txtArray.Skip(28).Take(txtArray.Length);
            values = values.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            for (int i = 0; i < values.Count(); i += keys.Count())
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                for (int j = 0; j < keys.Count(); j++)
                {
                    dict.Add(keys.ElementAt(j), values.ElementAt(i + j));
                }
                list.Add(dict);
            }
            return list;

        }

         private static void WriteListToFile(List<Dictionary<string, string>> list)
        {
            if (!Directory.Exists(pathToOutput)) Directory.CreateDirectory(pathToOutput);
            using StreamWriter file = File.CreateText(pathToOutput + "/data.json");
            var json = JsonConvert.SerializeObject(list);
            Console.WriteLine(json);
            file.Write(json.ToString());
        }
    }
}