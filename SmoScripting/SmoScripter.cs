using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SmoScripting
{
    public class SmoScripter
    {
        private readonly string dbServer;
        private readonly string dbName;
        private readonly string dbServerFolder;
        private readonly string dbFolder;
        private readonly string dbTablesFolder;
        private readonly string dbViewsFolder;
        private readonly string dbStoredProcFolder;
        private readonly string dbUserFunctionsFolder;
        private readonly string dbUserDefinedDataTypesFolder;
        private readonly string dbUserDefinedTableTypesFolder;
        private readonly string dbTableValuedFunctionsFolder;

        public SmoScripter(string dbName, string dbServer, string scriptsDir)
        {
            this.dbServer = dbServer;
            this.dbName = dbName;
            dbServerFolder = Path.Combine(scriptsDir, dbServer.Replace(".", ""));
            dbFolder = Path.Combine(dbServerFolder, dbName);
            dbTablesFolder = Path.Combine(dbFolder, "Tables");
            dbViewsFolder = Path.Combine(dbFolder, "Views");
            dbStoredProcFolder = Path.Combine(dbFolder, "StoredProcedures");
            dbUserFunctionsFolder = Path.Combine(dbFolder, "Functions");
            dbUserDefinedDataTypesFolder = Path.Combine(dbFolder, "UserDefinedDataTypes");
            dbUserDefinedTableTypesFolder = Path.Combine(dbFolder, "UserDefinedTableTypes");
            dbTableValuedFunctionsFolder = Path.Combine(dbFolder, "TableValuedFunctions");

        }

        public void GenerateScripts()
        {
            CreateDirectories();

            Server srv = new(dbServer);
            InitDefaultFields(srv);

            Database db = srv.Databases[dbName];

            Scripter scrp = new(srv);
            SetScripterOptions(scrp);

            ScriptObject(db, dbFolder, scrp, "Database");

            foreach (Table tb in db.Tables)
            {
                if (tb.IsSystemObject == false)
                {
                    ScriptObject(tb, dbTablesFolder, scrp);
                }
            }

            foreach(View view in db.Views)
            {
                if(view.IsSystemObject == false)
                {
                    ScriptObject(view, dbViewsFolder, scrp);
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

            foreach(UserDefinedDataType dataType in db.UserDefinedDataTypes)
            {
                ScriptObject(dataType, dbUserDefinedDataTypesFolder, scrp);
            }

            foreach (UserDefinedTableType tableType in db.UserDefinedTableTypes)
            {
                ScriptObject(tableType, dbUserDefinedTableTypesFolder, scrp);
            }
        }

        private static void SetScripterOptions(Scripter scrp)
        {
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
        }

        private static void InitDefaultFields(Server srv)
        {
            srv.GetDefaultInitFields(typeof(StoredProcedure));
            srv.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");

            srv.GetDefaultInitFields(typeof(UserDefinedFunction));
            srv.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");

            srv.GetDefaultInitFields(typeof(Table));
            srv.SetDefaultInitFields(typeof(Table), "IsSystemObject");
        }

        private void CreateDirectories()
        {
            Directory.CreateDirectory(dbServerFolder);
            Directory.CreateDirectory(dbFolder);
            Directory.CreateDirectory(dbViewsFolder);
            Directory.CreateDirectory(dbTablesFolder);
            Directory.CreateDirectory(dbUserFunctionsFolder);
            Directory.CreateDirectory(dbStoredProcFolder);
            Directory.CreateDirectory(dbUserDefinedDataTypesFolder);
            Directory.CreateDirectory(dbUserDefinedTableTypesFolder);

        }

        private static void ScriptObject(ScriptSchemaObjectBase sqlObject, string folder, Scripter scrp)
        {
            StringCollection scriptCollection = scrp.Script(new[] { sqlObject });
            IEnumerable<string> dbscripts = scriptCollection
                .Cast<string>()
                .AddAfterEachElement("GO");
            File.WriteAllLines(
                Path.Combine(folder, $"{sqlObject.Schema}.{sqlObject.Name}.sql"),
                dbscripts,
                Encoding.UTF8);
        }

        private static void ScriptObject(ScriptNameObjectBase sqlObject, string folder, Scripter scrp, string? fileName = null)
        {
            if(sqlObject is Database)
            {
                scrp.Options.WithDependencies = false;
            }
            StringCollection scriptCollection = scrp.Script(new[] { sqlObject });
            if (sqlObject is Database)
            {
                scrp.Options.WithDependencies = true;
            }
            IEnumerable<string> dbscripts = scriptCollection
                .Cast<string>()
                .AddAfterEachElement("GO");
            File.WriteAllLines(
                Path.Combine(folder, $"{fileName ?? sqlObject.Name}.sql"),
                dbscripts,
                Encoding.UTF8);
        }
    }
}