namespace ParkingDataSetup
{
    using IronPython.Hosting;
    using Microsoft.Scripting.Hosting;
    using System;
    using System.Collections.Generic;

    public static class ScriptEngine
    {
        public static string Execute(string subscriptionKey, string zipFile)
        {
            List<string> args = new List<string>();
            args.Insert(0, subscriptionKey);
            args.Insert(1, zipFile);

            var engine = Python.CreateEngine(); // Extract Python language engine from their grasp
            var scope = engine.CreateScope(); // Introduce Python namespace (scope)

                       // Print the default search paths
            System.Console.Out.WriteLine("Search paths:");
            ICollection<string> searchPaths = engine.GetSearchPaths();
            foreach (string path in searchPaths)
            {
                System.Console.Out.WriteLine(path);
            }
            System.Console.Out.WriteLine();

            // Now modify the search paths to include the directory
            // where the standard library has been installed
            searchPaths.Add("..\\..\\Lib");
            searchPaths.Add(@"C:\Loga\ParkingDataSetup\bin\Debug\netcoreapp3.1\Lib\site-packages");
            engine.SetSearchPaths(searchPaths);


            var argv = new Dictionary<string, object>
            {
                //Do some stuff and fill argv
                { "subscriptionKey","test" },{"zipFile","test" }
            }; // Add some sample parameters. Notice that there is no need in specifically setting the object type, interpreter will do that part for us in the script properly with high probability

            scope.SetVariable("argv", argv); // This will be the name of the dictionary in python script, initialized with previously created .NET Dictionary
            ScriptSource source = engine.CreateScriptSourceFromFile(@"C:\Loga\ParkingDataSetup\Script\DWGZipUploader.py"); // Load the script

            source.Execute(scope);
            zipFile = scope.GetVariable<string>("zipFile"); // To get the finally set variable 'parameter' from the python script
            return zipFile;
        }
    }
}
