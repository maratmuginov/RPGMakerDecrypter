using System;
using System.IO;
using System.Linq;
using CommandLine;
using RPGMakerDecrypter.Decrypter;
using RPGMakerDecrypter.Decrypter.Exceptions;

namespace RPGMakerDecrypter.Cli
{
    internal class Program
    {
        private static CommandLineOptions _commandLineOptions;

        private static void Main(string[] args)
        {
            _commandLineOptions = new CommandLineOptions();

            if (Parser.Default.ParseArguments(args, _commandLineOptions) == false)
            {
                Environment.Exit(1);
            }

            if (_commandLineOptions.InputPaths.Count == 0)
            {
                Console.WriteLine(_commandLineOptions.GetUsage());
                Environment.Exit(1);
            }

            var version = RGSSAD.GetVersion(_commandLineOptions.InputPaths.First());

            if (version == RPGMakerVersion.Invalid)
            {
                Console.WriteLine("Invalid input file.");
                Environment.Exit(1);
            }

            string outputDirectoryPath;

            if (_commandLineOptions.OutputDirectoryPath != null)
            {
                outputDirectoryPath = _commandLineOptions.OutputDirectoryPath;
            }
            else
            {
                FileInfo fi = new FileInfo(_commandLineOptions.InputPaths.First());
                outputDirectoryPath = fi.DirectoryName;
            }

            try
            {
                switch (version)
                {
                    case RPGMakerVersion.Xp:
                    case RPGMakerVersion.Vx:
                        new RGSSADv1(_commandLineOptions.InputPaths.First())
                            .ExtractAllFiles(outputDirectoryPath);
                        break;
                    case RPGMakerVersion.VxAce:
                        new RGSSADv3(_commandLineOptions.InputPaths.First())
                            .ExtractAllFiles(outputDirectoryPath);
                        break;
                }
            }
            catch (InvalidArchiveException)
            {
                Console.WriteLine("Archive is invalid or corrupted. Reading failed.");
                Environment.Exit(1);
            }
            catch (UnsupportedArchiveException)
            {
                Console.WriteLine("Archive is not supported or it is corrupted.");
                Environment.Exit(1);
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong with reading or extraction. Archive is likely invalid or corrupted.");
                Environment.Exit(1);
            }


            if (_commandLineOptions.GenerateProjectFile)
            {
                ProjectGenerator.GenerateProject(version, outputDirectoryPath);
            }
        }
    }
}
