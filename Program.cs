namespace ParkingDataSetup
{
    using IronPython.Hosting;
    using Microsoft.Scripting.Hosting;
    using System;
    using System.Collections.Generic;

    public class Program
    {
        public static void Main(string[] args)
        {
            ScriptRuntimeSetup setup = Python.CreateRuntimeSetup(null);
            ScriptRuntime runtime = new ScriptRuntime(setup);
            var engine = Python.GetEngine(runtime);
            ScriptSource source = engine.CreateScriptSourceFromFile(@"C:\Loga\ParkingDataSetup\Script\DWGZipUploader.py");
            ScriptScope scope = engine.CreateScope();

            // Print the default search paths
            ICollection<string> searchPaths = engine.GetSearchPaths();

            // Now modify the search paths to include the directory
            // where the standard library has been installed
            searchPaths.Add("..\\..\\Lib");
            searchPaths.Add(@"C:\Loga\ParkingDataSetup\bin\Debug\netcoreapp3.1\Lib\site-packages");
            engine.SetSearchPaths(searchPaths);

            var argv = new Dictionary<string, object>
            {
                //Do some stuff and fill argv
                { "subscriptionKey","test" },{"zipFile","test" }
            };

            engine.GetSysModule().SetVariable("argv", argv);
            var result = source.Execute(scope);

            Console.WriteLine("Hello World!");

            ScriptEngine.Execute(@"Qx05yzZN1EVBesESLh1CCFiiUSRbccCB-d5LP-sqkxE", @"C:\Loga\AzureMapsCreator-master\Sample - Contoso Drawing Package.zip");

            Console.WriteLine("Completed!");
        }
    }
}
