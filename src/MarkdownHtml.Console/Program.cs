using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MarkdownSharp;

namespace MarkdownHtml
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                WriteUsage("Please pass only one argument with the filepath to Markdown source file");
                return 1;
            }

            var sourceFile = Path.GetFullPath(args[0]);

            if (!File.Exists(sourceFile))
            {
                WriteUsage($@"Could not find Markdown source file at ""{sourceFile}""");
                return 2;
            }

            var templateFileInSourceFileDirectory = Path.Combine(
                Path.GetDirectoryName(sourceFile),
                "template.html");
            var templateFile = templateFileInSourceFileDirectory;

            if (!File.Exists(templateFileInSourceFileDirectory))
            {
                var templateFileInProgramDirectory = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "template.html");
                templateFile = templateFileInProgramDirectory;

                if (!File.Exists(templateFileInProgramDirectory))
                {
                    WriteUsage($@"Could not find Template file. Looked at the following paths:
    - ""{templateFileInSourceFileDirectory}""
    - ""{templateFileInProgramDirectory}""");
                    return 3;
                }
            }

            var markdown = new Markdown(new MarkdownOptions
            {
                AutoHyperlink = true,
                AutoNewLines = true,
                EmptyElementSuffix = "/>",
                EncodeProblemUrlCharacters = false,
                LinkEmails = true,
                StrictBoldItalic = false,
            });
            var sourceFileAllText = File.ReadAllText(sourceFile, Encoding.UTF8);
            var documentTitle = Path.GetFileNameWithoutExtension(sourceFile);
            var generator = $"markdown-html {GetAssemblyInformationalVersionValue()}";
            var templateFileAllText = File.ReadAllText(templateFile, Encoding.UTF8);
            var transformedHtml = markdown.Transform(sourceFileAllText);
            var targetFile = Path.ChangeExtension(sourceFile, "html");

            var combinedHtml = templateFileAllText;
            combinedHtml = Regex.Replace(combinedHtml, "{{ generator }}", generator);
            combinedHtml = Regex.Replace(combinedHtml, "{{ title }}", documentTitle);
            combinedHtml = Regex.Replace(combinedHtml, "{{ content }}", transformedHtml);

            File.WriteAllText(targetFile, combinedHtml, Encoding.UTF8);
            Console.WriteLine($@"Generated HTML file at ""{targetFile}""");

            return 0;
        }

        static void WriteUsage(string errorMessage)
        {
            Console.WriteLine("Error:");
            Console.WriteLine($"  {errorMessage}");
            Console.WriteLine();

            var version = GetAssemblyInformationalVersionValue();
            Console.WriteLine("Version:");
            Console.WriteLine($"  {version}");
            Console.WriteLine();

            Console.WriteLine("Usage:");
            Console.WriteLine(@"  markdown-html ""path\to\file.md""");
            Console.WriteLine();
            Console.WriteLine(@"  Outputs the converted HTML file at ""path\to\file.html""");
        }

        static string GetAssemblyInformationalVersionValue()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
        }
    }
}
