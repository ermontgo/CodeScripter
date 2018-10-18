using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TextCopy;

namespace CodeScripter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = "";
            if (args.Length > 0)
            {
                filePath = args[0];
            }
            else
            {
                Console.Write("Enter the script path: ");
                filePath = Console.ReadLine();
            }

            string workingDir = "";
            if (args.Length > 1)
            {
                workingDir = args[1];
            }
            else
            {
                Console.Write("Enter the code directory: ");
                workingDir = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(workingDir))
                {
                    Console.WriteLine($"Using {Environment.CurrentDirectory}");
                    workingDir = Environment.CurrentDirectory;
                }
            }

            string script = await File.ReadAllTextAsync(filePath);

            var runner = new ScriptRunner() { WorkingDirectory = workingDir };
            await runner.RunAsync(script);
                        
            Console.ReadKey();
        }
    }
}
