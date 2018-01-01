﻿using Monkeyspeak.Editor;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Logging
{
    internal class ConsoleWindowLogOutput : ConsoleLogOutput
    {
        private readonly ConsoleWindow console;

        public ConsoleWindowLogOutput(ConsoleWindow console)
        {
            this.console = console;
        }

        public override void Log(LogMessage logMsg)
        {
            if (logMsg.message == null) return;
            logMsg = BuildMessage(ref logMsg);
            Color color = Colors.White;
            switch (logMsg.Level)
            {
                case Level.Error:
                    color = Colors.Red;
                    break;

                case Level.Warning:
                    color = Colors.Yellow;
                    break;

                case Level.Debug:
                    color = Colors.Silver;
                    break;
            }
            console.Dispatcher.Invoke(() => console.WriteLine(logMsg.message, color));
        }
    }
}