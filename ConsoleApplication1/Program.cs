using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static readonly string[] FilePatterns =
        {
            "*.cs", "*.xml", "*.config", "*.asp", "*.aspx", "*.js", "*.htm", "*.html",
            "*.vb", "*.asax", "*.ashx", "*.asmx", "*.ascx", "*.master", "*.svc"
        }; //file extensions

        private static readonly string[] TextPatterns = {"ChildInterviewGrid.jpg"}; //Text to search

        private static void Main(string[] args)
        {
            try
            {
                var tfs = TfsTeamProjectCollectionFactory
                    .GetTeamProjectCollection(new Uri("http://cisrv048:8080/tfs/CI"));

                tfs.EnsureAuthenticated();

                var versionControl = tfs.GetService<VersionControlServer>();


                var outputFile = new StreamWriter(@"C:\temp\tfsfind.txt");
                var allProjs = versionControl.GetAllTeamProjects(true);
                foreach (var teamProj in allProjs)
                {
                    foreach (var filePattern in FilePatterns)
                    {
                        var items = versionControl.GetItems(teamProj.ServerItem + "/" + filePattern, RecursionType.Full)
                            .Items
                            .Where(i => !i.ServerItem.Contains("_ReSharper")); //skipping resharper stuff
                        foreach (var item in items)
                        {
                            var lines = SearchInFile(item);
                            if (lines.Count > 0)
                            {
                                outputFile.WriteLine("FILE:" + item.ServerItem);
                                outputFile.WriteLine(lines.Count + " occurence(s) found.");
                                outputFile.WriteLine();
                            }

                            foreach (var line in lines) outputFile.WriteLine(line);
                            if (lines.Count > 0) outputFile.WriteLine();
                        }
                    }

                    outputFile.Flush();
                }
            }
            catch (Exception e)
            {
                var ex = e.Message;
                Console.WriteLine("!!EXCEPTION: " + e.Message);
                Console.WriteLine("Stack Trace: " + e.StackTrace);
                Console.WriteLine("Continuing... ");
            }

            Console.WriteLine("========");
            Console.Read();
        }

        // Define other methods and classes here
        private static List<string> SearchInFile(Item file)
        {
            var result = new List<string>();

            try
            {
                var stream = new StreamReader(file.DownloadFile(), Encoding.Default);

                var line = stream.ReadLine();
                var lineIndex = 0;

                while (!stream.EndOfStream)
                {
                    if (TextPatterns.Any(p => line.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                        result.Add("=== Line " + lineIndex + ": " + line.Trim());

                    line = stream.ReadLine();
                    lineIndex++;
                }
            }
            catch (Exception e)
            {
                var ex = e.Message;
                Console.WriteLine("!!EXCEPTION: " + e.Message);
                Console.WriteLine("Continuing... ");
            }

            return result;
        }
    }
}