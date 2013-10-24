﻿using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace VSPackage.CPPCheckPlugin
{
    class AnalyzerCppcheck : ICodeAnalyzer
    {
        public override void analyze(List<SourceFile> filesToAnalyze, OutputWindowPane outputWindow, bool is64bitConfiguration)
        {
            Debug.Assert(_numCores > 0);
            String cppheckargs = "";

			HashSet<string> suppressions = new HashSet<string> { "cstyleCast", "missingIncludeSystem", "unusedStructMember", "unmatchedSuppression", "class_X_Y", "missingInclude", "constStatement", "unusedPrivateFunction"};

            // Creating the list of all different project locations (no duplicates)
            HashSet<string> projectPaths = new HashSet<string>(); // enforce uniqueness on the list of project paths
            foreach (var file in filesToAnalyze)
            {
                projectPaths.Add(file.BaseProjectPath);
            }

            // Creating the list of all different suppressions (no duplicates)
            foreach (var path in projectPaths)
            {
                suppressions.UnionWith(readSuppressions(path));
            }

			cppheckargs += (@"--enable=style,information,warning,performance,portability --inline-suppr -q --force --template=vs -j " + _numCores.ToString());
            foreach (string suppression in suppressions)
            {
                cppheckargs += (" --suppress=" + suppression);
            }

            // We only add include paths once, and then specify a set of files to check
			HashSet<string> includePaths = new HashSet<string>();
            foreach (var file in filesToAnalyze)
				foreach (string path in file.IncludePaths)
                {
					includePaths.Add(path);
                }

			foreach (string path in includePaths)
			{
				if (!path.ToLower().Contains("qt"))
				{
					String includeArgument = @" -I""" + path + @"""";
					cppheckargs = cppheckargs + " " + includeArgument;
				}
			}

			foreach (SourceFile file in filesToAnalyze)
			{
				cppheckargs += @" """ + file.FilePath + @"""";
			}

			if (filesToAnalyze.Count > 1) // For single file only checking current configuration (for speed)
			{
				// Creating the list of all different macros (no duplicates)
				HashSet<string> macros = new HashSet<string>();
				foreach (var file in filesToAnalyze)
				{
					foreach (string macro in file.Macros)
						macros.Add(macro);
				}
				macros.Add("_MSC_VER");
				macros.Add("__cplusplus");
				macros.Add("WIN32");
				macros.Add("_WIN32");
				if (is64bitConfiguration)
				{
					macros.Add("_M_X64");
					macros.Add("_WIN64");
				}
				else
				{
					macros.Add("_M_IX86");
				}

				foreach (string macro in macros)
				{
					String macroArgument = " -D" + macro;
					cppheckargs += macroArgument;
				}
			}

			string analyzerPath = Properties.Settings.Default.CPPcheckPath;
			while (!File.Exists(analyzerPath))
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "cppcheck executable|cppcheck.exe";
				if (dialog.ShowDialog() != DialogResult.OK)
					return;

				analyzerPath = dialog.FileName;
			}

			Properties.Settings.Default["CPPcheckPath"] = analyzerPath;
			Properties.Settings.Default.Save();
			run(analyzerPath, cppheckargs, outputWindow);
        }

		protected override HashSet<string> readSuppressions(string projectBasePath)
		{
			string settingsFilePath = projectBasePath + "\\suppressions.cfg";
			HashSet<string> suppressions = new HashSet<string>();
			if (File.Exists(settingsFilePath))
			{
				StreamReader stream = File.OpenText(settingsFilePath);
				string line = null;

				string currentGroup = "";
				while ((line = stream.ReadLine()) != null)
				{
					if (line.Contains("["))
					{
						currentGroup = line.Replace("[", "").Replace("]", "");
						continue; // to the next line
					}
					if (currentGroup == "cppcheck")
					{
						var components = line.Split(':');
// 						if (components.Length >= 2 && components[1] == "*")                          // id and "*" for a file specified
// 							components[1] = @"""" + projectBasePath + @"*""";                        // adding path to this specific project
						if (components.Length >= 2 && !components[1].StartsWith("*"))           // id and some path without "*"
							components[1] = "*" + components[1]; // adding * in front

						string suppression = components[0];
						if (components.Length > 1)
							suppression += ":" + components[1];
						if (components.Length > 2)
							suppression += ":"+ components[2];

						if (!string.IsNullOrEmpty(suppression))
							suppressions.Add(suppression.Replace("\\\\", "\\"));
					}
				}
			}

			return suppressions;
		}
    }
}