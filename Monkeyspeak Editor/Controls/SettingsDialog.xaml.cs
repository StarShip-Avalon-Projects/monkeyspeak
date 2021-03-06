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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Extensions;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Keybindings;
using Monkeyspeak.Editor.Plugins;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : MetroWindow
    {
        private ICSharpCode.AvalonEdit.TextEditor editorTemplate;

        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = this;
            UpdateDefaultStyle();

            settingsProps.Foreground = ThemeHelper.ToThemeForeground();
            settingsProps.Background = ThemeHelper.ToThemeBackground();
            syntaxProps.Foreground = ThemeHelper.ToThemeForeground();
            syntaxProps.Background = ThemeHelper.ToThemeBackground();
            pluginProps.Foreground = ThemeHelper.ToThemeForeground();
            pluginProps.Background = ThemeHelper.ToThemeBackground();
            optionsProps.Foreground = ThemeHelper.ToThemeForeground();
            optionsProps.Background = ThemeHelper.ToThemeBackground();

            settingsProps.AdvancedOptionsMenu = null;
            syntaxProps.AdvancedOptionsMenu = null;
            pluginProps.AdvancedOptionsMenu = null;
            optionsProps.AdvancedOptionsMenu = null;

            //settingsProps.AutoGenerateProperties = false;
            settingsProps.SelectedObject = new DictionaryPropertyGridAdapter<string, object>(Settings.Dictionary);

            foreach (var item in HighlightingManager.Instance.HighlightingDefinitions)
                syntax_categories.Items.Add(item.Name);
            syntax_categories.SelectionChanged += Syntax_categories_SelectionChanged;
            VisibilityHelper.SetIsVisible(syntax_tab, false);

            foreach (var plugin in PluginsManager.All)
                plugin_list.Items.Add(plugin.Name);
            plugin_list.SelectionChanged += Plugin_list_SelectionChanged;
            VisibilityHelper.SetIsVisible(plugin_tab, plugin_list.HasItems);

            editorTemplate = new ICSharpCode.AvalonEdit.TextEditor
            {
                Visibility = Visibility.Hidden,
                IsEnabled = false,
            };
            optionsProps.SelectedObject = editorTemplate.Options;

            HotkeyManager.PopulateKeybindingsConfiguration(this, hotkeysContainer, hotkeysContainer.FirstChild as StackPanel, hotkeysContainer.SecondChild as StackPanel);
        }

        private void Plugin_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0];
            var plugin = PluginsManager.All.FirstOrDefault(p => p.Name == (string)item);
            if (plugin != null)
                pluginProps.SelectedObject = plugin;
        }

        private void Syntax_categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0];
            var def = HighlightingManager.Instance.HighlightingDefinitions.First(d => d.Name == (string)item);
            syntaxProps.SelectedObject = new DictionaryPropertyGridAdapter<string, string>(def.Properties);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var editor in Editors.Instance.All)
            {
                editor.textEditor.Options = editorTemplate.Options;
            }
            HotkeyManager.Save();
            Settings.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}