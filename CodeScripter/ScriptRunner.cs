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

        private MarkdownCursor cursor;
        
        public async Task RunAsync(string script)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(script);
            
            Console.WriteLine($"Parsed {document.Blocks.Count} blocks");
            cursor = new MarkdownCursor(document.Blocks);

            MarkdownBlock block = cursor.Current;
            while (block != null)
            {
                switch (block)
                {
                    case CodeBlock cb:
                        HandleCodeBlock(cb); break;
                    default:
                        HandleTextBlock(block);
                        cursor.MoveNext();
                        break;
                }

                block = cursor.Current;
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

            char option = '\0';

            Console.Write("(c) copy; (e) execute; (n) next block, (b) previous block; (q) quit;\n> ");
            option = Console.ReadKey().KeyChar;

            // Move the cursor to overwrite the prompt
            // ResetCursor(2);
            Console.WriteLine();

            switch (option)
            {
                case 'c':
                    Clipboard.SetText(cb.Text);
                    cursor.MoveNext();
                    break;
                case 'e':
                    ExecuteCodeBlock(cb);
                    cursor.MoveNext();
                    break;
                case 'b': MoveToPreviousCodeBlock(); break;
                case 'n': cursor.MoveNext(); break;
                case 'q': cursor.End(); break;
            }
        }

        private void MoveToPreviousCodeBlock()
        {
            MarkdownBlock block = cursor.Current;
            do
            {
                cursor.MovePrevious();
                block = cursor.Current;
            } while (block != null && !(block is CodeBlock));

            if (block == null) cursor.MoveNext(); //If we hit the beginning, then start from the first one
        }

        private static void ResetCursor(int lineCount = 1)
        {
            for (int i = 0; i < lineCount; i++)
            {
                int cursorTop = Console.CursorTop;

                if (i > 0) cursorTop--;

                Console.SetCursorPosition(0, cursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, Console.CursorTop);
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
