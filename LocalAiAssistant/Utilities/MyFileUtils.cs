namespace LocalAiAssistant.Utilities
{
#pragma warning disable IDE0051 // Remove unused private members
    internal class MyFileUtils
    {
        public MyFileUtils()
        {
        }
        public static string ReadTextFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }
        public static void WriteTextFile(string filePath, string text)
        {
            File.WriteAllText(filePath, text);
        }
        public static bool SearchTextInFile(string filePath, string searchString)
        {
            string fileText = File.ReadAllText(filePath);
            return fileText.Contains(searchString);
        }
        public static byte[] ReadBinaryFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        public static void WriteBinaryFile(string filePath, byte[] data)
        {
            File.WriteAllBytes(filePath, data);
        }
        public static void AppendTextToFile(string filePath, string text)
        {
            File.AppendAllText(filePath, text);
        }
        public static void AppendBinaryToFile(string filePath, byte[] data)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Append))
            {
                fs.Write(data, 0, data.Length);
            }
        }
        private static string GetFilePath(string fileName)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, fileName);
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}