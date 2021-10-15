using System;
using System.IO;
namespace JSONReducer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string[] imageFiles = GetFileNames("G:\\Projects\\Fusee\\Examples\\Complete\\MuVista\\Core\\Assets\\Panos", "*.jpg");



        }
    private static string[] GetFileNames(string path, string filter)
    {
        string[] files = Directory.GetFiles(path, filter);
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileName(files[i]);
        return files;
    }
}
}
