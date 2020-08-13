﻿using System;
using System.Collections.Generic;
using System.IO;

namespace DocsPortingTool
{
    public class Configuration
    {
        private static readonly char Separator = ',';

        private enum Mode
        {
            DisablePrompts,
            Docs,
            ExceptionCollisionThreshold,
            ExcludedAssemblies,
            ExcludedTypes,
            IncludedAssemblies,
            IncludedTypes,
            Initial,
            PortExceptionsExisting,
            PortExceptionsNew,
            PortMemberParams,
            PortMemberProperties,
            PortMemberReturns,
            PortMemberRemarks,
            PortMemberSummaries,
            PortMemberTypeParams,
            PortTypeParams, // Params of a Type
            PortTypeRemarks,
            PortTypeSummaries,
            PortTypeTypeParams, // TypeParams of a Type
            PrintUndoc,
            Save,
            SkipInterfaceImplementations,
            SkipInterfaceRemarks,
            TripleSlash
        }

        public static readonly string ToBeAdded = "To be added.";

        public static readonly string[] ForbiddenDirectories = new[] { "binplacePackages", "docs", "mscorlib", "native", "netfx", "netstandard", "pkg", "Product", "ref", "runtime", "shimsTargetRuntime", "testhost", "tests", "winrt" };

        public List<DirectoryInfo> DirsTripleSlashXmls { get; } = new List<DirectoryInfo>();
        public List<DirectoryInfo> DirsDocsXml { get; } = new List<DirectoryInfo>();

        public HashSet<string> IncludedAssemblies { get; } = new HashSet<string>();
        public HashSet<string> ExcludedAssemblies { get; } = new HashSet<string>();
        public HashSet<string> IncludedTypes { get; } = new HashSet<string>();
        public HashSet<string> ExcludedTypes { get; } = new HashSet<string>();

        public bool DisablePrompts { get; set; } = false;
        public int ExceptionCollisionThreshold { get; set; } = 70;
        public bool PortExceptionsExisting { get; set; } = false;
        public bool PortExceptionsNew { get; set; } = true;
        public bool PortMemberParams { get; set; } = true;
        public bool PortMemberProperties { get; set; } = true;
        public bool PortMemberReturns { get; set; } = true;
        public bool PortMemberRemarks { get; set; } = true;
        public bool PortMemberSummaries { get; set; } = true;
        public bool PortMemberTypeParams { get; set; } = true;
        /// <summary>
        /// Params of a Type.
        /// </summary>
        public bool PortTypeParams { get; set; } = true;
        public bool PortTypeRemarks { get; set; } = true;
        public bool PortTypeSummaries { get; set; } = true;
        /// <summary>
        /// TypeParams of a Type.
        /// </summary>
        public bool PortTypeTypeParams { get; set; } = true;
        public bool PrintUndoc { get; set; } = false;
        public bool Save { get; set; } = false;
        public bool SkipInterfaceImplementations { get; set; } = false;
        public bool SkipInterfaceRemarks { get; set; } = true;

        public static Configuration GetFromCommandLineArguments(string[] args)
        {
            Mode mode = Mode.Initial;

            Log.Info("Verifying CLI arguments...");

            if (args == null || args.Length == 0)
            {
                Log.LogErrorPrintHelpAndExit("No arguments passed to the executable.");
            }

            Configuration config = new Configuration();

            foreach (string arg in args!)
            {
                switch (mode)
                {
                    case Mode.DisablePrompts:
                        {
                            config.DisablePrompts = ParseOrExit(arg, "Disable prompts");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.Docs:
                        {
                            string[] splittedDirPaths = arg.Split(',', StringSplitOptions.RemoveEmptyEntries);

                            Log.Cyan($"Specified Docs xml locations:");
                            foreach (string dirPath in splittedDirPaths)
                            {
                                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                                if (!dirInfo.Exists)
                                {
                                    Log.LogErrorAndExit($"This Docs xml directory does not exist: {dirPath}");
                                }

                                config.DirsDocsXml.Add(dirInfo);
                                Log.Info($"  -  {dirPath}");
                            }

                            mode = Mode.Initial;
                            break;

                        }

                    case Mode.ExceptionCollisionThreshold:
                        {
                            if (!int.TryParse(arg, out int value))
                            {
                                Log.LogErrorAndExit($"Invalid int value for 'Exception collision threshold' argument: {arg}");
                            }
                            else if (value < 1 || value > 100)
                            {
                                Log.LogErrorAndExit($"Value needs to be between 0 and 100: {value}");
                            }

                            config.ExceptionCollisionThreshold = value;

                            Log.Cyan($"Exception collision threshold:");
                            Log.Info($" - {value}");
                            break;
                        }

                    case Mode.ExcludedAssemblies:
                        {
                            string[] splittedArg = arg.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

                            if (splittedArg.Length > 0)
                            {
                                Log.Cyan("Excluded assemblies:");
                                foreach (string assembly in splittedArg)
                                {
                                    Log.Cyan($" - {assembly}");
                                    config.ExcludedAssemblies.Add(assembly);
                                }
                            }
                            else
                            {
                                Log.LogErrorPrintHelpAndExit("You must specify at least one assembly.");
                            }

                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.ExcludedTypes:
                        {
                            string[] splittedArg = arg.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

                            if (splittedArg.Length > 0)
                            {
                                Log.Cyan($"Excluded types:");
                                foreach (string typeName in splittedArg)
                                {
                                    Log.Cyan($" - {typeName}");
                                    config.ExcludedTypes.Add(typeName);
                                }
                            }
                            else
                            {
                                Log.LogErrorPrintHelpAndExit("You must specify at least one type name.");
                            }

                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.IncludedAssemblies:
                        {
                            string[] splittedArg = arg.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

                            if (splittedArg.Length > 0)
                            {
                                Log.Cyan($"Included assemblies:");
                                foreach (string assembly in splittedArg)
                                {
                                    Log.Cyan($" - {assembly}");
                                    config.IncludedAssemblies.Add(assembly);
                                }
                            }
                            else
                            {
                                Log.LogErrorPrintHelpAndExit("You must specify at least one assembly.");
                            }

                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.IncludedTypes:
                        {
                            string[] splittedArg = arg.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

                            if (splittedArg.Length > 0)
                            {
                                Log.Cyan($"Included types:");
                                foreach (string typeName in splittedArg)
                                {
                                    Log.Cyan($" - {typeName}");
                                    config.IncludedTypes.Add(typeName);
                                }
                            }
                            else
                            {
                                Log.LogErrorPrintHelpAndExit("You must specify at least one type name.");
                            }

                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.Initial:
                        {
                            switch (arg.ToUpperInvariant())
                            {
                                case "-DOCS":
                                    mode = Mode.Docs;
                                    break;

                                case "-DISABLEPROMPTS":
                                    mode = Mode.DisablePrompts;
                                    break;

                                case "EXCEPTIONCOLLISIONTHRESHOLD":
                                    mode = Mode.ExceptionCollisionThreshold;
                                    break;

                                case "-EXCLUDEDASSEMBLIES":
                                    mode = Mode.ExcludedAssemblies;
                                    break;

                                case "-EXCLUDEDTYPES":
                                    mode = Mode.ExcludedTypes;
                                    break;

                                case "-H":
                                case "-HELP":
                                    Log.PrintHelp();
                                    Environment.Exit(0);
                                    break;

                                case "-INCLUDEDASSEMBLIES":
                                    mode = Mode.IncludedAssemblies;
                                    break;

                                case "-INCLUDEDTYPES":
                                    mode = Mode.IncludedTypes;
                                    break;

                                case "-PORTEXCEPTIONSEXISTING":
                                    mode = Mode.PortExceptionsExisting;
                                    break;

                                case "-PORTEXCEPTIONSNEW":
                                    mode = Mode.PortExceptionsNew;
                                    break;

                                case "-PORTMEMBERPARAMS":
                                    mode = Mode.PortMemberParams;
                                    break;

                                case "-PORTMEMBERPROPERTIES":
                                    mode = Mode.PortMemberProperties;
                                    break;

                                case "-PORTMEMBERRETURNS":
                                    mode = Mode.PortMemberReturns;
                                    break;

                                case "-PORTMEMBERREMARKS":
                                    mode = Mode.PortMemberRemarks;
                                    break;

                                case "-PORTMEMBERSUMMARIES":
                                    mode = Mode.PortMemberSummaries;
                                    break;

                                case "-PORTMEMBERTYPEPARAMS":
                                    mode = Mode.PortMemberTypeParams;
                                    break;

                                case "-PORTTYPEPARAMS": // Params of a Type
                                    mode = Mode.PortTypeParams;
                                    break;

                                case "-PORTTYPEREMARKS":
                                    mode = Mode.PortTypeRemarks;
                                    break;

                                case "-PORTTYPESUMMARIES":
                                    mode = Mode.PortTypeSummaries;
                                    break;

                                case "-PORTTYPETYPEPARAMS": // TypeParams of a Type
                                    mode = Mode.PortTypeTypeParams;
                                    break;

                                case "-PRINTUNDOC":
                                    mode = Mode.PrintUndoc;
                                    break;

                                case "-SAVE":
                                    mode = Mode.Save;
                                    break;

                                case "-SKIPINTERFACEIMPLEMENTATIONS":
                                    mode = Mode.SkipInterfaceImplementations;
                                    break;

                                case "-SKIPINTERFACEREMARKS":
                                    mode = Mode.SkipInterfaceRemarks;
                                    break;

                                case "-TRIPLESLASH":
                                    mode = Mode.TripleSlash;
                                    break;
                                default:
                                    Log.LogErrorPrintHelpAndExit($"Unrecognized argument: {arg}");
                                    break;
                            }
                            break;
                        }

                    case Mode.PortExceptionsExisting:
                        {
                            config.PortExceptionsExisting = ParseOrExit(arg, "Port existing exceptions");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortExceptionsNew:
                        {
                            config.PortExceptionsNew = ParseOrExit(arg, "Port new exceptions");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberParams:
                        {
                            config.PortMemberParams = ParseOrExit(arg, "Port member Params");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberProperties:
                        {
                            config.PortMemberProperties = ParseOrExit(arg, "Port member Properties");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberRemarks:
                        {
                            config.PortMemberRemarks = ParseOrExit(arg, "Port member Remarks");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberReturns:
                        {
                            config.PortMemberReturns = ParseOrExit(arg, "Port member Returns");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberSummaries:
                        {
                            config.PortMemberSummaries = ParseOrExit(arg, "Port member Summaries");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortMemberTypeParams:
                        {
                            config.PortMemberTypeParams = ParseOrExit(arg, "Port member TypeParams");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortTypeParams: // Params of a Type
                        {
                            config.PortTypeParams = ParseOrExit(arg, "Port Type Params");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortTypeRemarks:
                        {
                            config.PortTypeRemarks = ParseOrExit(arg, "Port Type Remarks");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortTypeSummaries:
                        {
                            config.PortTypeSummaries = ParseOrExit(arg, "Port Type Summaries");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PortTypeTypeParams: // TypeParams of a Type
                        {
                            config.PortTypeTypeParams = ParseOrExit(arg, "Port Type TypeParams");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.PrintUndoc:
                        {
                            config.PrintUndoc = ParseOrExit(arg, "Print undoc");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.Save:
                        {
                            config.Save = ParseOrExit(arg, "Save");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.SkipInterfaceImplementations:
                        {
                            config.SkipInterfaceImplementations = ParseOrExit(arg, "Skip interface implementations");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.SkipInterfaceRemarks:
                        {
                            config.SkipInterfaceRemarks = ParseOrExit(arg, "Skip appending interface remarks");
                            mode = Mode.Initial;
                            break;
                        }

                    case Mode.TripleSlash:
                        {
                            string[] splittedDirPaths = arg.Split(',', StringSplitOptions.RemoveEmptyEntries);

                            Log.Cyan($"Specified triple slash locations:");
                            foreach (string dirPath in splittedDirPaths)
                            {
                                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                                if (!dirInfo.Exists)
                                {
                                    Log.LogErrorAndExit($"This triple slash xml directory does not exist: {dirPath}");
                                }

                                config.DirsTripleSlashXmls.Add(dirInfo);
                                Log.Info($"  -  {dirPath}");
                            }

                            mode = Mode.Initial;
                            break;
                        }

                    default:
                        {
                            Log.LogErrorPrintHelpAndExit("Unexpected mode.");
                            break;
                        }
                }
            }

            if (mode != Mode.Initial)
            {
                Log.LogErrorPrintHelpAndExit("You missed an argument value.");
            }

            if (config.DirsDocsXml == null)
            {
                Log.LogErrorPrintHelpAndExit("You must specify a path to the dotnet-api-docs xml folder with -docs.");
            }

            if (config.DirsTripleSlashXmls.Count == 0)
            {
                Log.LogErrorPrintHelpAndExit("You must specify at least one triple slash xml folder path with -tripleslash.");
            }

            if (config.IncludedAssemblies.Count == 0)
            {
                Log.LogErrorPrintHelpAndExit("You must specify at least one assembly with -include.");
            }

            return config;
        }

        // Hardcoded namespaces that need to be renamed to what MS Docs uses.
        public static string ReplaceNamespace(string str)
        {
            return str.Replace("Microsoft.Data", "System.Data");
        }

        // Tries to parse the user argument string as boolean, and if it fails, exits the program.
        private static bool ParseOrExit(string arg, string paramFriendlyName)
        {
            if (!bool.TryParse(arg, out bool value))
            {
                Log.LogErrorAndExit($"Invalid boolean value for '{paramFriendlyName}' argument: {arg}");
            }

            Log.Cyan($"{paramFriendlyName}:");
            Log.Info($" - {value}");

            return value;
        }
    }
}
