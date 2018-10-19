using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace CodeScripter
{
    public class ScriptRunner
    {
        public string WorkingDirectory { get; set; }
        
        public async Task RunAsync(string script)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(script);

            Console.WriteLine($"Parsed {document.Blocks.Count} blocks");
            foreach (var block in document.Blocks)
            {
                switch (block)
                {
                    case CodeBlock cb:
                        HandleCodeBlock(cb); break;
                    default: HandleTextBlock(block); break;
                }
            }

            Console.WriteLine("All blocks processed");
        }

        private void HandleTextBlock(MarkdownBlock block)
        {
            var oldColor = Console.ForegroundColor;

            if (block is HeaderBlock header)
            {
                if (header.HeaderLevel == 1) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (header.HeaderLevel == 2) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.Magenta;
            }

            Console.WriteLine(block.ToString());

            Console.ForegroundColor = oldColor;
        }

        private void HandleCodeBlock(CodeBlock cb)
        {
            PrintCodeBlockHeader(cb);

            string option = "";

            do
            {
                Console.WriteLine("Enter (c) to copy; (e) to execute; (n) to go to the next block");
                option = Console.ReadLine();

                if (option == "c")
                {
                    Clipboard.SetText(cb.Text);
                    break;
                }
                else if (option == "e")
                {
                    ExecuteCodeBlock(cb);
                    break;
                }
            } while (option != "n");
        }

        private void PrintCodeBlockHeader(CodeBlock cb)
        {
            var oldColor = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGreen;

            Console.Write($"[CODE ({cb.CodeLanguage ?? "snippet"})] ");
            string slug = cb.Text.Trim();
            
            if (slug.Length > 60)
            {
                slug = slug.Substring(0, 60) + "...";
            }

            int newlineIdx = slug.IndexOf('\n');
            if (newlineIdx > -1)
            {
                slug = slug.Substring(0, newlineIdx);
            }

            Console.WriteLine(slug);

            Console.BackgroundColor = oldColor;
        }

        private void ExecuteCodeBlock(CodeBlock cb)
        {
            if (cb.CodeLanguage == "git-sha")
            {
                string commit = cb.Text;
                Console.WriteLine($"Moving to commit ${commit}");
                ProcessCommand("git checkout -- .");
                ProcessCommand("git clean -fd");
                ProcessCommand($"git checkout {commit}");
            }
            else if (cb.CodeLanguage == "cmd")
            {
                ProcessCommand(cb.Text);
            }
        }

        private void ProcessCommand(string text)
        {
            Console.WriteLine($"Executing {text}");
            var escapedArgs = text.Replace("\"", "\\\"");

            var info = new ProcessStartInfo("cmd.exe", $"/c \"{escapedArgs}\"") { WorkingDirectory = WorkingDirectory };
            Process.Start(info).WaitForExit(15000);
        }
    }
}
