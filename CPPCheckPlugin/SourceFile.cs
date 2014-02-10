﻿using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;

namespace VSPackage.CPPCheckPlugin
{
	class SourceFile
	{
		public enum VCCompilerVersion { vc2010, vc2012, vc2013, vcFuture };

		public SourceFile(string fullPath, string projectBasePath, string vcCompilerName)
		{
			_fullPath = cleanPath(fullPath);
			_projectBasePath = cleanPath(projectBasePath);

			if (vcCompilerName.Contains("2010"))
				_compilerVersion = VCCompilerVersion.vc2010;
			else if (vcCompilerName.Contains("2012"))
				_compilerVersion = VCCompilerVersion.vc2012;
			else if (vcCompilerName.Contains("2013"))
				_compilerVersion = VCCompilerVersion.vc2013;
			else
				_compilerVersion = VCCompilerVersion.vcFuture;
		}

		// All include paths being added are resolved against projectBasePath
		public void addIncludePath(string path)
		{
			if (!String.IsNullOrEmpty(_projectBasePath) && !String.IsNullOrEmpty(path) && !path.Equals(".") && !path.Equals("\\\".\\\""))
			{
				Debug.WriteLine("Processing path: " + path);				
				if (path.Contains("\\:")) // absolute path
					_includePaths.Add(cleanPath(path));
				else
				{
					// Relative path - converting to absolute
					String pathForCombine = path.Replace("\"", String.Empty).TrimStart('\\', '/');
					_includePaths.Add(cleanPath(Path.GetFullPath(Path.Combine(_projectBasePath, pathForCombine)))); // Workaround for Path.Combine bugs
				}
			}
		}

		public void addIncludePaths(List<string> paths)
		{
			foreach (string path in paths)
			{
				addIncludePath(path);
			}
		}

		public void addMacro(string macro)
		{
			_activeMacros.Add(macro);
		}

		public void addMacros(List<string> macros)
		{
			foreach (string macro in macros)
			{
				addMacro(macro);
			}
		}

		public string FilePath
		{
			set { Debug.Assert(_fullPath == null); _fullPath = cleanPath(value); } // Only makes sense to set this once, a second set call is probably a mistake
			get { return _fullPath; }
		}

		public string RelativeFilePath
		{
			get { return cleanPath(_fullPath.Replace(_projectBasePath, "")); }
		}

		public string BaseProjectPath
		{
			set { Debug.Assert(_projectBasePath == null); _projectBasePath = cleanPath(value); } // Only makes sense to set this once, a second set call is probably a mistake
			get { return _projectBasePath; }
		}

		public List<string> IncludePaths
		{
			get { return _includePaths; }
		}

		public List<string> Macros
		{
			get { return _activeMacros; }
		}

		public VCCompilerVersion vcCompilerVersion
		{
			get { return _compilerVersion; }
		}

		private static string cleanPath(string path)
		{
			string result = path.Replace("\"", "").Replace("\\\\", "\\");
			if (result.EndsWith("\\"))
				result = result.Substring(0, result.Length - 1);
			if (result.StartsWith("\\"))
				result = result.Substring(1);
			return result;
		}

		private string _fullPath = null;
		private string _projectBasePath = null;
		private List<string> _includePaths = new List<string>();
		private List<string> _activeMacros = new List<string>();
		private VCCompilerVersion _compilerVersion;
	}
}
