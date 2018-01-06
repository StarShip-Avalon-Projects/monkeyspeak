﻿using ICSharpCode.AvalonEdit.Utils;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Interfaces.Console;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : MetroWindow, IConsole
    {
        private Paragraph paragraph;
        internal List<IConsoleCommand> commands;
        internal LinkedList<string> history;

        public ConsoleWindow()
        {
            InitializeComponent();
            this.paragraph = new Paragraph();
            console.Document = new FlowDocument(paragraph);
            history = new LinkedList<string>();
            commands = new List<IConsoleCommand>();
            foreach (var asm in ReflectionHelper.GetAllAssemblies())
            {
                foreach (var type in ReflectionHelper.GetAllTypesWithInterface<IConsoleCommand>(asm))
                {
                    if (ReflectionHelper.HasNoArgConstructor(type))
                    {
                        if (ReflectionHelper.TryCreate(type, out var consoleCommand))
                        {
                            commands.Add((IConsoleCommand)consoleCommand);
                        }
                    }
                }
            }
            DataContext = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }

        public void Write(string output, Color color)
        {
            paragraph.Inlines.Add(new Run(output)
            {
                FontFamily = console.FontFamily,
                FontStyle = System.Windows.FontStyles.Normal,
                FontWeight = System.Windows.FontWeights.Normal,
                Foreground = new SolidColorBrush(color)
            });
            //paragraph.Inlines.Add(new LineBreak());
            scroll.ScrollToEnd();
        }

        public void WriteLine(string output, Color color)
        {
            paragraph.Inlines.Add(new Run(output)
            {
                FontFamily = console.FontFamily,
                FontStyle = System.Windows.FontStyles.Normal,
                FontWeight = System.Windows.FontWeights.Normal,
                Foreground = new SolidColorBrush(color)
            });
            paragraph.Inlines.Add(new LineBreak());
            scroll.ScrollToEnd();
            DataContext = this;
        }

        private LinkedListNode<string> node;

        private void input_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                if (history.Contains(input.Text))
                    history.Remove(input.Text);
                history.AddFirst(input.Text);
                if (history.Count > 10)
                {
                    while (history.Count > 10) history.RemoveLast();
                }
                var commandsFound = commands.FindAll(c => input.Text.StartsWith(c.Command, StringComparison.InvariantCultureIgnoreCase));
                if (commandsFound.Count > 0)
                {
                    foreach (var command in commandsFound)
                    {
                        try
                        {
                            command.Invoke(this, Editors.Instance.Selected,
                                input.Text.Substring(command.Command.Length).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0]);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to execute {command.Command}\n{ex}");
                        }
                    }
                    input.Text = null;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                e.Handled = true;
                if (history.Count > 0)
                {
                    if (node == null) node = history.First;
                    else
                    {
                        node = node.Next;
                    }
                    input.Text = node?.Value;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                e.Handled = true;
                if (history.Count > 0)
                {
                    if (node == null) node = history.Last;
                    else
                    {
                        node = node.Previous;
                    }
                    input.Text = node?.Value;
                }
            }
        }
    }
}