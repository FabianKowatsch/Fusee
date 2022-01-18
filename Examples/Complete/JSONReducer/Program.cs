using Fusee.Math.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace JSONReducer
{
    class Program
    {
        private static string pathToImages = "D:\\Projects\\Fusee\\Examples\\Complete\\MuVista\\Core\\Assets\\Panos";
        private static string pathToInput = "D:\\Projects\\Fusee\\Examples\\Complete\\JSONReducer\\input\\data.json";
        private static string pathToOutput = "D:\\Projects\\Fusee\\Examples\\Complete\\JSONReducer\\output";
        static void Main(string[] args)
        {
            string[] imageFiles = GetFileNames(pathToImages, "*.jpg");

            //removeJPGString(imageFiles);

            List<PanoImage> newPanoList = alteredPanoList(imageFiles);

            foreach (PanoImage img in newPanoList)
            {
                Console.WriteLine(img.filename);
            }

            writeListToFile(newPanoList);


        }
        private static string[] GetFileNames(string path, string filter)
        {
            string[] files = Directory.GetFiles(path, filter);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }

        private static void removeJPGString(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Substring(0, array[i].Length - 4);
                Console.WriteLine(array[i]);
            }
        }
        private static List<PanoImage> alteredPanoList(string[] filenames)
        {
            string json = File.ReadAllText(pathToInput);
            var panos = JsonConvert.DeserializeObject<List<PanoImage>>(json);

            var filteredPanos = from p in panos
                                where filenames.Contains(p.filename)
                                select p;

            return filteredPanos.ToList();
        }

        private static void writeListToFile(List<PanoImage> newPanos)
        {
            if (!Directory.Exists(pathToOutput)) Directory.CreateDirectory(pathToOutput);
            using StreamWriter file = File.CreateText(pathToOutput + "/data.json");
            var json = JsonConvert.SerializeObject(newPanos);
            file.Write(json.ToString());
        }
    }
}