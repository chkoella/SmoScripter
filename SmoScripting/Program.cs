using System;
using System.IO;
using System.Reflection;

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
            string scriptsDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            SmoExample.GenerateScripts(dbName, server, scriptsDir);
            Console.WriteLine($"Duration: {DateTime.Now-start}");
        }
    }
}