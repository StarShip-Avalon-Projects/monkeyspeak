﻿using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ConsoleWindow console;

        public MainWindow()
        {
            InitializeComponent();
            //Logger.SuppressSpam = true;
            console = new ConsoleWindow();
            Logger.LogOutput = new MultiLogOutput(new ConsoleWindowLogOutput(console),
                new NotificationPanelLogOutput(Level.Error));

            NotificationManager.Added += notif => this.Dispatcher.Invoke(() => notif_badge.Badge = NotificationManager.Count);
            NotificationManager.Removed += notif => this.Dispatcher.Invoke(() => notif_badge.Badge = NotificationManager.Count);
            NotificationManager.Added += notif =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    notifs_list.Items.Add(new NotificationPanel(notif));
                    notifs_list.ScrollIntoView(notifs_list.Items[notifs_list.Items.Count - 1]);
                });
            };

            Editors.Added += editor => this.Dispatcher.Invoke(() => docs.Items.Add(editor));
            Editors.Removed += editor => this.Dispatcher.Invoke(() => docs.Items.Remove(editor));

            foreach (var col in Enum.GetNames(typeof(AppColor)))
            {
                style_chooser.Items.Add(col);
            }

            Logger.Error("TEST");
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            notifs_flyout.AutoCloseInterval = 3000;
            notifs_flyout.IsAutoCloseEnabled = false;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            console.Close();
        }

        private void Console_Click(object sender, RoutedEventArgs e)
        {
            if (console.Visibility != Visibility.Visible)
            {
                console.Show();
            }
            else
            {
                console.Hide();
            }
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            notifs_flyout.IsOpen = !notifs_flyout.IsOpen;
        }

        private void Notifications_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.Dispatcher.Invoke(() =>
                {
                    notifs_flyout.IsOpen = false;
                    NotificationManager.Clear();
                });
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void notifs_flyout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/captkirk88/monkeyspeak");
        }

        private void TriggerList_SelectionChanged(KeyValuePair<string, string> kv)
        {
            if (Controls.EditorControl.Selected != null)
            {
                Controls.EditorControl.Selected.InsertLine(Controls.EditorControl.Selected.CaretLine, kv.Key);
            }
        }

        private void style_chooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse(style_chooser.SelectedItem.ToString(), out AppColor col))
            {
                SetColor(col);
            }
        }

        public void SetColor(AppColor color)
        {
            this.Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current.Resources,
                                        ThemeManager.GetAccent(Enum.GetName(typeof(AppColor), color)),
                                        appStyle.Item1);
            });
        }

        public AppColor GetColor()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            if (Enum.TryParse(appStyle.Item2.Name, out AppColor color))
                return color;
            else return AppColor.Brown;
        }

        public void SetTheme(AppTheme accent)
        {
            this.Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current.Resources,
                                        appStyle.Item2,
                                        ThemeManager.GetAppTheme($"Base{Enum.GetName(typeof(AppTheme), accent)}"));
            });
        }

        public AppTheme GetTheme()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            Enum.TryParse(appStyle.Item1.Name.Replace("Base", ""), out AppTheme theme);
            return theme;
        }
    }
}