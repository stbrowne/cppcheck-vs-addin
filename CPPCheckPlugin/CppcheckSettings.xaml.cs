﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VSPackage.CPPCheckPlugin
{
	/// <summary>
	/// Interaction logic for CppcheckSettings.xaml
	/// </summary>
	public partial class CppcheckSettings : Window
	{
		public static string DefaultArguments = "--inline-suppr -v --template=\"{file}|{line}|{severity}|{id}|{message}\" --library=windows --library=std --language=c++";

		public CppcheckSettings()
		{
			InitializeComponent();
			Activated += onActivated;
			Closed += OnClosed;

			if (String.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultArguments))
				Properties.Settings.Default.DefaultArguments = DefaultArguments;
			else
				Properties.Settings.Default.DefaultArguments = Properties.Settings.Default.DefaultArguments.Replace("--template=vs", "--template=\"{file}|{line}|{severity}|{id}|{message}\"");

			Properties.Settings.Default.Save();

			if (!Properties.Settings.Default.CheckSavedFilesHasValue)
			{
				CPPCheckPluginPackage.askCheckSavedFiles();
			}
		}

		private void onActivated(object o, EventArgs e)
		{
			var settings = Properties.Settings.Default;

			InconclusiveChecks.IsChecked = settings.InconclusiveChecksEnabled;

			if (!settings.CheckSavedFilesHasValue)
				CheckSavedFiles.IsChecked = false;
			else
				CheckSavedFiles.IsChecked = settings.CheckSavedFiles;
			Project_OnlyCheckCurrentConfig.IsChecked = settings.ProjectOnlyCheckCurrentConfig;
			File_OnlyCheckCurrentConfig.IsChecked = settings.FileOnlyCheckCurrentConfig;
			ArgumentsEditor.Text = settings.DefaultArguments;
            IgnoreIncludePaths.IsChecked = settings.IgnoreIncludePaths;

            var radios = new Dictionary<RadioButton, ICodeAnalyzer.ShowOutputWindow>() {
                { ProblemFoundRadio, ICodeAnalyzer.ShowOutputWindow.ProblemFound },
                { CheckCompleteRadio, ICodeAnalyzer.ShowOutputWindow.CheckComplete },
                { NeverShowRadio, ICodeAnalyzer.ShowOutputWindow.Never }
            };
            foreach (var radio in radios)
            {
                radio.Key.Tag = radio.Value;
                radio.Key.IsChecked = (settings.ShowOutputWindow == (int)radio.Value);
            }
		}

		private void OnClosed(object o, EventArgs e)
		{
			Properties.Settings.Default.DefaultArguments = String.IsNullOrEmpty(ArgumentsEditor.Text) ? DefaultArguments.Replace('\n', ' ').Replace('\r', ' ') : ArgumentsEditor.Text;
			Properties.Settings.Default.Save();
		}

		private void inconclusive_Unchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.InconclusiveChecksEnabled = false;
			Properties.Settings.Default.Save();
		}

		private void inconclusive_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.InconclusiveChecksEnabled = true;
			Properties.Settings.Default.Save();
		}

		private void checkSavedFiles_Unchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.CheckSavedFiles = false;
			Properties.Settings.Default.Save();
		}

		private void checkSavedFiles_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.CheckSavedFiles = true;
			Properties.Settings.Default.Save();
		}

        private void ignoreIncludePaths_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IgnoreIncludePaths = false;
            Properties.Settings.Default.Save();
        }

        private void ignoreIncludePaths_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IgnoreIncludePaths = true;
            Properties.Settings.Default.Save();
        }

        private void onDefaultArguments(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.DefaultArguments = DefaultArguments;
			ArgumentsEditor.Text = DefaultArguments;
			Properties.Settings.Default.Save();
		}

		private void Project_OnlyCheckCurrentConfig_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.ProjectOnlyCheckCurrentConfig = true;
			Properties.Settings.Default.Save();
		}

		private void Project_OnlyCheckCurrentConfig_Unchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.ProjectOnlyCheckCurrentConfig = false;
			Properties.Settings.Default.Save();
		}

		private void File_OnlyCheckCurrentConfig_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.FileOnlyCheckCurrentConfig = true;
			Properties.Settings.Default.Save();
		}

		private void File_OnlyCheckCurrentConfig_Unchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.FileOnlyCheckCurrentConfig = false;
			Properties.Settings.Default.Save();
		}

		private void EditGlobalSuppressions(object sender, RoutedEventArgs e)
		{
			var settings = new SuppressionSettingsUI.SuppressionsSettings(ICodeAnalyzer.SuppressionStorage.Global);
			settings.ShowDialog();
		}

		private void EditSolutionSuppressions(object sender, RoutedEventArgs e)
		{
			var settings = new SuppressionSettingsUI.SuppressionsSettings(ICodeAnalyzer.SuppressionStorage.Solution);
			settings.ShowDialog();
		}

		private void EditProjectSuppressions(object sender, RoutedEventArgs e)
		{
			var projectPath = CPPCheckPluginPackage.activeProjectPath();
			var projectName = CPPCheckPluginPackage.activeProjectName();
			if (projectPath != "" && projectName != "")
			{
				var settings = new SuppressionSettingsUI.SuppressionsSettings(
					ICodeAnalyzer.SuppressionStorage.Project,
					projectPath,
					projectName);
				settings.ShowDialog();
			}
			else
				MessageBox.Show("No C++ project selected in the solution explorer.");
		}

		private void MessagesListClick(object sender, RoutedEventArgs e)
		{
			var messagesListWindow = new CppcheckMessagesList();
			messagesListWindow.Show();
		}

        private void ShowOutputRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == null)
                return;

            Properties.Settings.Default.ShowOutputWindow = (int)radio.Tag;
            Properties.Settings.Default.Save();
        }
	}
}
