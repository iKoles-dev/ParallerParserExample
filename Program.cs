using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace TGC
{
    using Homebrew;

    class Program
    {
        private static int _currentThreadCount = 0;
        private static int _maxThreadValue = 500;
        private static List<string> _urls = new List<string>();
        private static List<string> _blackList = new List<string>();

        static Stack<string> _good = new Stack<string>();
        static Stack<string> _errors = new Stack<string>();
        static Stack<string> _bl = new Stack<string>();
        static int _channels = 0;

        static void Main(string[] args)
        {
            Proxies.Load();
            if (args.Length < 1)
            {
                Console.WriteLine("No files uploaded. Just Drag'andDrop txt them to this exe file.");
                Console.ReadLine();
                return;
            }

            SetBlackList();
            SetUrlsList(args);

            Console.WriteLine();
            Console.WriteLine("Uploaded file : {0}", args.Length);
            Console.WriteLine("Uploaded line : {0}", _urls.Count);
            Console.WriteLine("Uploaded black list : {0}", _blackList.Count);
            Console.WriteLine();

            StartParsing();
            Save();


            Console.WriteLine();
            Console.WriteLine("Find chat : {0}", _good.Count);
            Console.WriteLine("Channel : {0}", _channels);
            Console.WriteLine("Error : {0}", _errors.Count);

            Console.WriteLine("\n\nPress <Any key> to quit.");
            Console.ReadKey();
        }

        private static void SetBlackList()
        {
            string blackListPath = @"blacklist.txt";
            if (!File.Exists(blackListPath))
            {
                File.Create(blackListPath).Dispose();
            }
            else
            {
                string rawBlackList = File.ReadAllText(blackListPath);
                if (!string.IsNullOrEmpty(rawBlackList))
                {
                    _blackList = new List<string>(rawBlackList.Split("\n"));
                }
            }
        }
        private static void SetUrlsList(string[] args)
        {
            foreach (string path in args)
            {
                string[] rawUrls = File.ReadAllText(path).Split("\r\n");
                foreach (var url in rawUrls)
                {
                    if (_urls.Contains(url) || _blackList.Contains(url)
                                            || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        continue;
                    }
                    _urls.Add(url);
                }
            }
        }
        private static void StartParsing()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            _urls.ForEach(
                url =>
                    {
                        while (_currentThreadCount >= _maxThreadValue)
                        {
                            Thread.Sleep(100);
                        }

                        string tempUrl = url;
                        _currentThreadCount++;
                        new Thread(
                            () =>
                                {
                                    TelegramChecker telegramChecker = new TelegramChecker(tempUrl);
                                    CheckStatus(telegramChecker);
                                    _currentThreadCount--;
                                }).Start();
                    });

            while (_currentThreadCount != 0)
            {
                Thread.Sleep(100);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine("Runtime : {0}", elapsedTime);
        }
        private static void Save()
        {
            string black_list_path = @"blacklist.txt", output_path, errorPath = @"error.txt";
            output_path = String.Format("{0}.txt", DateTime.Now.ToString("HH_mm dd_MM_yy"));

            if (!File.Exists(output_path)) { using (FileStream fs = File.Create(output_path)) { }; }
            File.WriteAllLines(output_path, _good);

            if (!File.Exists(black_list_path)) { using (FileStream fs = File.Create(black_list_path)) { }; }
            File.AppendAllLines(black_list_path, _bl);

            if (!File.Exists(errorPath)) { using (FileStream fs = File.Create(errorPath)) { }; }
            File.AppendAllLines(errorPath, _bl);
        }
        private static void CheckStatus(TelegramChecker telegramChecker)
        {
            if (telegramChecker.IsError)
            {
                Console.WriteLine($"{telegramChecker.Url} | Error");
                _errors.Push(telegramChecker.Url);
                return;
            }
            switch (telegramChecker.UrlType)
            {
                case UrlTypes.Channel:
                    {
                        Console.WriteLine($"{telegramChecker.Url} | channel");
                        _bl.Push(telegramChecker.Url);
                        _channels++;
                        break;
                    }
                case UrlTypes.CloseChat:
                    {
                        string data = $"{telegramChecker.Url} | Close chat";
                        _good.Push(data);
                        break;
                    }
                case UrlTypes.Chat:
                    {
                        string data = $"{telegramChecker.Url} | {telegramChecker.Members}";
                        Console.WriteLine(data);
                        _bl.Push(telegramChecker.Url);
                        break;
                    }
            }
        }
    }
}