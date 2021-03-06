﻿using Monkeyspeak;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using Monkeyspeak.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using Monkeyspeak.Lexical.Expressions;
using System.Collections.Generic;
using Monkeyspeak.Lexical;
using System.Net.NetworkInformation;
using System.Net;
using Monkeyspeak.Tests;
using Monkeyspeak.Extensions;
using Microsoft.CSharp.RuntimeBinder;

namespace MonkeyspeakTests

{
    public class MyLibrary : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            // add trigger handlers here
            Add(TriggerCategory.Cause, 0, EntryPointForScript,
                "when the script starts,");
        }

        private bool EntryPointForScript(TriggerReader reader)
        {
            return true; // return false stops execution of any triggers below the one that called this method
        }

        public override void Unload(Page page)
        {
            // this is called by page.Dispose() which is not called automatically remove any
            // unmanaged and disposable resources here or just due a ending action
        }
    }

    [TestFixture]
    public class UnitTest1
    {
        private string testScript = @"
(0:0) when the script is started,
    (5:102) print {Welcome to %MONKEY} to the console.

(0:0) when the script is started,
        (5:101) set %num to 0x333.
        (5:102) print {num = %num} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {this will be false move on to next condition}
		(5:100) set %hello to {Hello World}.
        (5:101) set %helloNum to 5.69.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console.

(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:101) set %num to 5.1212E+003.
        (5:102) print {num = %num} to the console.
        (5:102) print {%hello} to the console.

(0:0) when the script is started,
		(5:250) create a table as %multi
		(5:101) set variable %multi[name][test] to 3
		(5:102) print {%multi[name][test]} to the log.

(0:0) when the script is started,
    *Uncommented version
    *(1:104) and variable %hello equals {this will be false move on to next condition}
     *   (5:102) print {the pen is blue!} to the console
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {the pen is red!} to the console
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:100) set %hello to {@%helloNum}
        (5:101) set %helloNum to 5.6969.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:115) call job 1 with %helloNum and {test arg}.
        (5:115) call job 2.

(0:0) when the script is started,
        (5:250) create a table as %myTable.
        (5:100) set %myTable[myKey] to {Hello World}.
        (5:102) print {%myTable[myKey]} to the console.
        (6:250) for each entry in table %myTable put it into %entry,
            (5:102) print {%entry} to the console.

(0:100) when job 1 is called put arguments into table %table,
    	(5:102) print {job 1 executed} to the console
		(5:101) set variable %counter to 1.
	(6:453) while variable %counter is less than 10000,
		(5:150) take variable %counter and add 1 to it.
	(6:457) after the loop is done,
		(5:102) print {counter = %counter} to the log.

(0:100) when job 2 is called,
    (5:102) print {job will not execute because infinite loop possibility} to the console
   (5:115) call job 1.
";

        public static string tableScript = @"
(0:0) when the script is started,
    (5:250) create a table as %myTable.
    (5:100) set %hello to {hi}
    (5:100) set %myTable[1] to {%hello}
    (5:100) set %myTable[2] to {%hello}
    (5:100) set %myTable[3] to {%hello}
    (5:100) set %myTable[123] to {1}
    (6:250) for each entry in table %myTable put it into %entry,
        (5:102) print {%entry} to the console.
    (6:457) after the loop is done,
        (5:150) take variable %myTable[123] and add 1 to it.
        (5:102) print {%myTable[123]} to the console.
        (5:254) with table %myTable create a copy between keys {1} and {3} and put it into table %copy
    (6:250) for each entry in table %copy put it into %entry,
        (5:102) print {Copy: %entry} to the console.
    (6:457) after the loop is done,
        (6:456) repeat 6 times,
            (5:102) print {weeee!} to the console.

(0:0) when the script is started,
    (5:101) set %answer to 0
    (5:101) set %life to 42
    (6:450) while variable %answer is not %life,
        (5:150) take variable %answer and add 1 to it.
        (1:102) and variable %answer equals 21,
            (5:450) exit the current loop.
        (1:103) and variable %answer does not equal 21,
            (5:150) take variable %answer and add 1 to it.
    (6:457) after the loop is done,
        (5:102) print {We may never know the answer...but the answer right now is %answer} to the console.

(0:0) when the script is started,
    (5:250) create a table as %mytable
    (5:100) set %myTable[123] to {123}
    (6:250) for each entry in table %mytable put it into %entry,
        (5:102) print {%entry} to the console.
    (6:457) after the loop is done,
        (5:102) print {%mytable[123]} to the console.
";

        public static string testGerolkaeTemplate = @"
*MSPK V04.00 Silver Monkey
*MonkeySpeak Script File
*Created by <name>

*Endtriggers* 8888 *Endtriggers*
";

        public static string objVarScript = @"
(0:0) when the script is started,
    (5:100) set %hello to {Hello World}.
    (5:270) create a object variable %objVar1
    (5:100) set variable %objVar1 to {test}.
    (5:102) print {%objVar1} to the log.

(0:0) when the script is started,
    (5:102) print {objVar2.Test = %objVar2.Test} to the log.
    (5:101) set variable %objVar2.Test to 321.
    (5:102) print {objVar2.Test = %objVar2.Test} to the log.
";

        [SetUp]
        public void Initialize()
        {
            Logger.SingleThreaded = true;
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void AssemblyInfo()
        {
            var asm = Assembly.GetAssembly(typeof(MonkeyspeakEngine));
            Logger.Info($"Assembly Name: {asm.FullName}");
            Logger.Info($".NET Version: {asm.ImageRuntimeVersion}");
            Logger.Info($"Version: {asm.GetName().Version}");
        }

        [Test]
        public void Tables()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;

            StringBuilder sb = new StringBuilder();
            var lines = tableScript.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    sb.Append($"[{i + 1}]").AppendLine(lines[i]);
            }
            Logger.Info(sb.ToString());

            Page page = engine.LoadFromString(tableScript); // replace with tableScriptMini to see results of that script

            page.Error += DebugAllErrors;
            page.AddTriggerHandler(TriggerCategory.Condition, 666, AlwaysFalseCond);
            page.LoadAllLibraries();
            page.SetVariable(new Variable("%testVariable", "Hello WOrld", true));

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Logger.Assert(page.Size > 0, "Page size was 0 = FAIL!");
            page.Execute();
            foreach (var variable in page.Scope)
            {
                Logger.Info($"{variable.ToString()} {variable.GetType().Name}");
            }
        }

        [Test]
        public void NewStringTriggersTest()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            //Logger.LogOutput = new MultiLogOutput(new FileLogger(Level.Debug), new FileLogger(), new FileLogger(Level.Info));
            var script = @"
(0:0) when the script starts,
		(5:100) set variable %str to {I am a string, I am a string!}.
        (5:102) print {Testing with '%str'} to the log.
		(5:400) with {%str} get word count and put it into variable %words.
		(5:102) print {%words} to the log.
		(5:401) with {%str} get words starting at 2 to 5 and put it into variable %am.
		(5:102) print {%am} to the log.
		(5:402) with {%str} get index of {%am} and put it intovariable %index.
		(5:102) print {%index} to the log.
		(5:403) with {%str} replace all occurances of {a} with {the} and put it into variable %replace.
		(5:102) print {%replace} to the log.
		(5:404) with {%str} get everything left of {string} and put it into variable %iama
		(5:102) print {%iama} to the log.
		(5:405) with {%str} get everything right most left of {a} and put it into variable %IamastringIam
		(5:102) print {%IamastringIam} to the log.
		(5:406) with {%str} get everything right of {,} and put it into variable %Iamastring
		(5:102) print {%Iamastring} to the log.
		(5:407) with {%str} get everything far right of {a} and put it into variable %string
		(5:102) print {%string} to the log.
		(5:408) with {%str} split it at each { } and put it into table %split
	(6:250) for each entry in table %split put it into %entry,
		(5:102) print {%entry} to the log.
	(6:454) after the loop is done,
		(5:253) with table %split join the contents and put it into variable %joined
		(5:102) print {%joined} to the log.
";
            Page page = engine.LoadFromString(script);

            page.LoadAllLibraries();

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Assert.Greater(page.Size, 0);
            page.Error += DebugAllErrors;
            page.Execute();
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        //[Test]
        public async Task DemoTest()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            //Logger.LogOutput = new MultiLogOutput(new FileLogger(Level.Debug), new FileLogger(), new FileLogger(Level.Info));
            var script = @"
(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.

(0:0) when the script is started,
        (5:101) set %hex to 0x333.
        (5:101) set %million to 1,000,000.
    (1:102) and variable %million equals 1,000,000,
        (5:102) print {\!million = %million} to the console.
    (1:102) and variable %hex equals 0x333
        (5:102) print {hex = %hex} to the console.
    (1:102) and variable %hex equals 2
        (5:102) print {wtf} to the console.
        (5:102) print {1} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {Hello World}
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {Will show} to the console.
        (5:102) print {2} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {Hello World}
    (1:104) and variable %hello equals {this will be false move on to next condition}
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {Will not show even though the first was true} to the console.
        (5:102) print {should not show} to the console.
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {3} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {this will be false move on to next condition}
    (1:104) and variable %hello equals {Minty!}
        (5:102) print {Will not show} to the console.
        (5:102) print {4} to the console.
";
            Page page = engine.LoadFromString(script);

            page.LoadAllLibraries();
            //page.LoadDebugLibrary();
            page.SetVariable(new Variable("%testVariable", "Hello WOrld", true));

            page.AddTriggerHandler(TriggerCategory.Cause, 666, AlwaysFalseCond);

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Assert.Greater(page.Size, 0);
            page.Error += DebugAllErrors;
            Logger.Debug("Creating cancellation token");
            page.Execute();
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        [Test]
        public async Task AsyncDemoTest()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.LoopLimit = int.MaxValue;
            Page page = await engine.LoadFromStringAsync(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            //page.LoadDebugLibrary();
            page.SetVariable(new Variable("%testVariable", "Hello WOrld", true));

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Logger.Assert(page.Size > 0, "Page size was 0 = FAIL!");
            await page.ExecuteAsync();
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        //[Test]
        //public async Task ObjectVariables()
        //{
        //    var engine = new MonkeyspeakEngine();
        //    engine.Options.Debug = true;
        //    //Logger.LogOutput = new MultiLogOutput(new FileLogger(Level.Debug), new FileLogger(), new FileLogger(Level.Info));

        //    Page page = engine.LoadFromString(objVarScript);

        //    page.LoadAllLibraries();

        //    var objVar = (ObjectVariable)page.SetVariable(new ObjectVariable("%objVar2"));
        //    dynamic value = objVar.DynamicValue;
        //    value.Test = 123d;

        //    // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
        //    Console.WriteLine("Trigger Count: " + page.Size);
        //    Assert.Greater(page.Size, 0);
        //    page.Error += DebugAllErrors;

        //    page.Execute();
        //    foreach (var variable in page.Scope)
        //    {
        //        Console.WriteLine(variable.ToString());
        //    }
        //}

        [Test]
        public void LexerPrint()
        {
            //using (var stream = new FileStream("testBIG.ms", FileMode.OpenOrCreate))
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(objVarScript)))
            using (Lexer lexer = new Lexer(new MonkeyspeakEngine(), new SStreamReader(stream)))
            {
                foreach (var token in lexer.ReadToEnd())
                {
                    if (token.Type != TokenType.COMMENT)
                        Logger.Info($"{token} = {new string(lexer.Read(token.ValueStartPosition, token.Length))}");
                }
            }
        }

        [Test]
        public void LexerAndParserPrint()
        {
            Logger.Debug(default(Trigger));
            var engine = new MonkeyspeakEngine();
            //engine.Options.Debug = true;
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(objVarScript)))
            using (Lexer lexer = new Lexer(engine, new SStreamReader(stream)))
            {
                Parser parser = new Parser(engine);
                //parser.VisitToken = VisitTokens;
                foreach (var trigger in parser.Parse(lexer))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(trigger.ToString(engine, true));
                    Logger.Info(sb.ToString());
                }
                parser.VisitToken = null;
            }
        }

        [Test]
        public void ExpressionsReplaceTest()
        {
            var testScript = @"
(0:90) When the bot enters a Dream,
(0:1) When the bot logs into furcadia,
(5:6) whisper {Bot active in dream %DREAMNAME} to {%BOTCONTROLLER}.

(0:10) When someone shouts something with {fuck} in it,
(5:5) whisper {Please do not swear in shouts! Thank you #SA} to the triggering furre.
";
            var engine = new MonkeyspeakEngine();
            Expressions.Instance.Replace<MyFakeStringExpression>(TokenType.STRING_LITERAL);
            //engine.Options.Debug = true;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(testScript));
            using (Lexer lexer = new Lexer(engine, new SStreamReader(stream)))
            {
                Parser parser = new Parser(engine);
                //parser.VisitToken = VisitTokens;
                foreach (var trigger in parser.Parse(lexer))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(trigger.ToString(engine, true));
                    Logger.Info(sb.ToString());
                }
                parser.VisitToken = null;
            }
        }

        private class MyFakeStringExpression : Expression<string>
        {
            public MyFakeStringExpression()
            {
            }

            public MyFakeStringExpression(SourcePosition pos, string val) : base(pos, val)
            {
                SetValue("LOL!");
            }

            public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
            {
                return "LOL!";
            }
        }

        [Test]
        public void DebugTest()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.LoopLimit = int.MaxValue;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            //page.LoadDebugLibrary();

            var var = page.SetVariable(new Variable("%testVariable", "Hello WOrld", true));

            Console.WriteLine("Trigger Count: " + page.Size);
            using (PerfCounter perf = new PerfCounter((time, mem) => Logger.Info($"{time}\n{mem}")))
                page.Execute(0);
        }

        [Test]
        public void DurabilityParseFile()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = false;
            Logger.InfoEnabled = false;
            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;
            using (var perf = new PerfCounter((elapsed, mem) =>
            {
                Logger.InfoEnabled = true;
                Logger.Info($"{elapsed} {mem}");
                Logger.InfoEnabled = false;
            }, TimeSpan.FromMilliseconds(100)))
            {
                Page page = engine.LoadFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testBIG.ms"));

                page.LoadAllLibraries();
                page.Error += DebugAllErrors;
                Logger.InfoEnabled = true;
                Logger.Info("Trigger Count: " + page.Size);
                Logger.InfoEnabled = false;
                page.Execute();
                Logger.Info($"Trigger Count: {page.Size}");
            }
        }

        [Test]
        public async Task DurabilityParseFileAsync()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            Page page = await engine.LoadFromFileAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testBIG.ms"));

            page.Debug = true;

            page.LoadAllLibraries();

            page.Error += DebugAllErrors;

            Console.WriteLine("Trigger Count: " + page.Size);
            var timer = Stopwatch.StartNew();
            await page.ExecuteAsync();
            Logger.Info($"Trigger Count: {page.Size}");
        }

        [Test]
        public void DurabilityParseString()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;
            engine.Options.Debug = false;
            Logger.InfoEnabled = false;
            var sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.AppendLine();
                sb.AppendLine("(0:0) when the script is started,");
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
                sb.AppendLine("(1:104) and variable %hello equals {this will be false move on to next condition}");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine("(1:104) and variable %hello equals {Hello World}");
                sb.AppendLine("(5:101) set %helloNum to 5.");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = %helloNum} to the console.");
                sb.AppendLine();
            }
            //Logger.Info(sb.ToString());
            using (var perf = new PerfCounter((elapsed, mem) =>
            {
                Logger.InfoEnabled = true;
                Logger.Info($"{elapsed} {mem}");
                Logger.InfoEnabled = false;
            }, TimeSpan.FromMilliseconds(100)))
            {
                Page page = engine.LoadFromString(sb.ToString());
                page.LoadAllLibraries();

                page.Error += DebugAllErrors;

                Logger.InfoEnabled = true;
                Logger.Info($"Triggers: {page.Size}");
                Logger.InfoEnabled = false;

                page.Execute();
            }
        }

        [Test]
        public void DurabilityParseStringAsync()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
            }

            Page page = engine.LoadFromString(sb.ToString());

            page.LoadAllLibraries();

            var sb2 = new StringBuilder();
            for (int i = 0; i < 50000; i++)
            {
                sb2.AppendLine("(5:100) set %hello to {Hello World}.");
            }

            var script = sb2.ToString();
            var tasks = new Task[5];
            for (int i = 0; i <= tasks.Length - 1; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using (var mon = new PerfCounter((time, mem) => Logger.Info($"{time}\n{mem}")))
                        await engine.LoadFromStringAsync(page, script);
                }).ContinueWith(task => Logger.Info($"Triggers: {page.Size}"));
            }

            page.Error += DebugAllErrors;

            Task.WaitAll(tasks);
            Logger.Info($"Triggers: {page.Size}");
            //page.Execute(0);
        }

        [Test]
        public void SetGetVariableTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            engine.Options.VariableCountLimit = 100000;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            for (int i = 0; i <= 10000; i++)
                page.SetVariable(new Variable(i.ToString(), true.ToString(), false));
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        [Test]
        public void IOLibraryTest()
        {
            var ioTestString = @"
(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.
	(5:102) print {%file} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak %VERSION!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
";
            var engine = new MonkeyspeakEngine();
            var page = engine.LoadFromString(ioTestString);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();

            page.Execute(0);
            foreach (var v in page.Scope)
            {
                Logger.Debug(v);
            }
        }

        [Test]
        public void GetTriggerDescriptionsTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();

            foreach (var desc in page.GetTriggerDescriptions().OrderBy(d => d.Item1.GetType().Name).OrderBy(d => d.Item2.Category).OrderBy(d => d.Item2.Id))
            {
                Console.WriteLine($"{desc.Item3}");
            }
        }

        [Test]
        public void TimerLibraryTest()
        {
            var timerLibTestScript = @"
(0:0) when the script starts,
    (5:303) pause script execution for 2 seconds.
    (5:304) get the current time in seconds and put it into variable %currentTime.
    (5:102) print {Current Time %currentTime secs} to the console.

(0:0) when the script starts,
    (5:101) set variable %timer to 1.
    (5:300) create timer %timer to go off every 2 second(s) with a start delay of 1 second(s).
    (5:300) create timer 2 to go off every 5 second(s). *don't need delay part here
    (5:300) create timer 13 to go off every 900 second(s) with a start delay of # second(s).

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {Timer %timerId at %currentTime secs} to the console.

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {timerId = %timerId at %currentTime secs} to the console.
    (5:150) take variable %i2 and add 1 to it.
    (5:102) print {elapsed count = %i2} to the console.
";
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            var page = engine.LoadFromString(timerLibTestScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();

            page.Execute(0);
            Thread.Sleep(10000);
            page.Dispose();
        }

        [Test]
        public void NonExistantTimerTest()
        {
            var timerLibTestScript = @"

(0:0) when the script starts,
    (5:300) create timer 13 to go off every 900 second(s) with a start delay of 0 second(s).
    (5:102) print {Timer 13 created} to the console.

(0:300) when timer 2 goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {Timer %timerId at %currentTime secs} to the console.
";
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            var page = engine.LoadFromString(timerLibTestScript);

            //page.Error += DebugAllErrors;

            page.LoadAllLibraries();

            page.Execute(0);
            System.Threading.Thread.Sleep(100);
            page.Dispose();
        }

        [Test]
        public void NonExistantTriggersTest()
        {
            var testScript = @"
";
            var engine = new MonkeyspeakEngine();
            Assert.Throws<NullReferenceException>(() => engine.LoadFromString(testScript));
        }

        [Test]
        public void CallEffectTest()
        {
            var engine = new MonkeyspeakEngine();
            var page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();

            page.Execute();
            System.Threading.Thread.Sleep(100);
            page.Dispose();
        }

        public static bool HandleScriptStartCause(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        public static bool AlwaysFalseCond(TriggerReader reader)
        {
            return false;
        }

        private void DebugAllErrors(Page page, TriggerHandler handler, Monkeyspeak.Trigger trigger, Exception ex)
        {
            Logger.Error($"{handler.Method.Name} in {trigger.ToString(page.Engine)}\n{ex}");
        }

        public static Token VisitTokens(ref Token token)
        {
            return new Token(TokenType.NONE, token.ValueStartPosition, token.Length, token.Position); // lolololol!
        }
    }
}