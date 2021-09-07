namespace SmoScripting
{
    class Program
    {
        static void Main(string[] args)
        {
            var start = DateTime.Now;

            //string server = args[0];
            //string dbName = args[1];
            //string scriptsDir = args[2];
            string server = "localhost";
            string dbName = "ScriptTest";
            string scriptsDir = GetApplicationRoot();

            SmoScripter scripter = new(dbName, server, scriptsDir);
            scripter.GenerateScripts();
            Console.WriteLine($"Duration: {DateTime.Now - start}");
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                ?? throw new Exception("Failed to determine the application root directory");      
        }
    }
}