using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Text;

namespace SmoScripting
{
    public static class SmoExample
    {
        public static void GenerateScripts(string dbName, string dbServer, string scriptsDir)
        {
            string dbServerFolder = Path.Combine
            (scriptsDir, dbServer.Replace(".", ""));
            string dbFolder = Path.Combine(dbServerFolder, dbName);
            string dbTablesFolder = Path.Combine(dbFolder, "Tables");
            string dbStoredProcFolder = Path.Combine(dbFolder, "StoredProcedures");
            string dbUserFunctionsFolder = Path.Combine(dbFolder, "Functions");

            Directory.CreateDirectory(dbServerFolder);
            Directory.CreateDirectory(dbFolder);
            Directory.CreateDirectory(dbTablesFolder);
            Directory.CreateDirectory(dbUserFunctionsFolder);
            Directory.CreateDirectory(dbStoredProcFolder);

            Server srv = new Server(dbServer);
            srv.GetDefaultInitFields(typeof(StoredProcedure));
            srv.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");
            srv.GetDefaultInitFields(typeof(UserDefinedFunction));
            srv.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");
            srv.GetDefaultInitFields(typeof(Table));
            srv.SetDefaultInitFields(typeof(Table), "IsSystemObject");

            Database db = srv.Databases[dbName];

            Scripter scrp = new Scripter(srv);
            scrp.Options.ScriptDrops = false;
            scrp.Options.WithDependencies = true;
            scrp.Options.Indexes = true;
            scrp.Options.DriAllConstraints = true;
            scrp.Options.NoCommandTerminator = true;
            scrp.Options.AllowSystemObjects = true;
            scrp.Options.Permissions = true;
            scrp.Options.DriAllConstraints = true;
            scrp.Options.SchemaQualify = true;
            scrp.Options.AnsiFile = true;
            scrp.Options.DriIndexes = true;
            scrp.Options.DriClustered = true;
            scrp.Options.DriNonClustered = true;
            scrp.Options.NonClusteredIndexes = true;
            scrp.Options.ClusteredIndexes = true;
            scrp.Options.FullTextIndexes = true;
            scrp.Options.EnforceScriptingOptions = true;
            scrp.Options.IncludeHeaders = false;
            scrp.Options.ScriptBatchTerminator = true;
            scrp.Options.Triggers = true;

            foreach (Table tb in db.Tables)
            {
                if (tb.IsSystemObject == false)
                {
                    ScriptObject(tb, dbTablesFolder, scrp);
                }
            }

            foreach (StoredProcedure storedProc in db.StoredProcedures)
            {
                if (storedProc.IsSystemObject == false)
                {
                    ScriptObject(storedProc, dbStoredProcFolder, scrp);
                }
            }

            foreach (UserDefinedFunction function in db.UserDefinedFunctions)
            {
                if (function.IsSystemObject == false)
                {
                    ScriptObject(function, dbUserFunctionsFolder, scrp);
                }
            }
        }

        private static void ScriptObject(ScriptSchemaObjectBase sqlObject, string folder, Scripter scrp)
        {
            StringCollection scriptCollection = scrp.Script(new[] { sqlObject });
            var dbscripts = scriptCollection
                .Cast<string>()
                .AddAfterEachElement("GO");
            File.WriteAllLines(
                Path.Combine(folder, $"{sqlObject.Schema}.{sqlObject.Name}.sql"),
                dbscripts,
                Encoding.UTF8);
        }
    }
}