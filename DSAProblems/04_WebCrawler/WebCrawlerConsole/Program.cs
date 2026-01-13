using System.Diagnostics;
using WebCrawlerConsole.BusinessLogic;
using WebCrawlerConsole.Interfaces;

namespace WebCrawlerConsole;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Multi-Threaded Web Crawler - Learning Project           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. Run Example 1 (Yahoo News - 4 URLs expected)");
            Console.WriteLine("2. Run Example 2 (Google - 1 URL expected)");
            Console.WriteLine("3. Benchmark (Compare single vs multi-threaded)");
            Console.WriteLine("4. Custom crawl");
            Console.WriteLine("5. Exit");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await RunExample1();
                        break;
                    case "2":
                        await RunExample2();
                        break;
                    case "3":
                        await RunBenchmark();
                        break;
                    case "4":
                        await RunCustomCrawl();
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option!");
                        break;
                }
            }
            catch (NotImplementedException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Crawler not implemented yet!");
                Console.WriteLine("💡 See SingleThreadedCrawler.cs or MultiThreadedCrawler.cs");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine("\nGoodbye! 👋");
    }

    static async Task RunExample1()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("Example 1: Yahoo News Crawl");
        Console.WriteLine(new string('═', 60));

        var startUrl = "http://news.yahoo.com/news/topics/";
        var parser = MockHtmlParser.CreateExample1();

        await RunCrawl(startUrl, parser, "Example 1", expectedCount: 4);
    }

    static async Task RunExample2()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("Example 2: Google News Crawl");
        Console.WriteLine(new string('═', 60));

        var startUrl = "http://news.google.com";
        var parser = MockHtmlParser.CreateExample2();

        await RunCrawl(startUrl, parser, "Example 2", expectedCount: 1);
    }

    static async Task RunCrawl(string startUrl, IHtmlParser parser, string testName, int expectedCount)
    {
        Console.WriteLine($"\nStart URL: {startUrl}");
        Console.Write("Use multi-threading? (y/n): ");
        var useMulti = Console.ReadLine()?.ToLower() == "y";

        IWebCrawler crawler = useMulti
            ? new MultiThreadedCrawler(maxConcurrency: 10)
            : new SingleThreadedCrawler();

        Console.WriteLine($"\nCrawling with {(useMulti ? "Multi-Threaded" : "Single-Threaded")} crawler...");
        
        var sw = Stopwatch.StartNew();
        var results = await crawler.CrawlAsync(startUrl, parser);
        sw.Stop();

        Console.WriteLine("\n" + new string('─', 60));
        Console.WriteLine("Results:");
        Console.WriteLine(new string('─', 60));

        foreach (var url in results.OrderBy(u => u))
        {
            Console.WriteLine($"  ✓ {url}");
        }

        Console.WriteLine(new string('─', 60));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"URLs Found:      {results.Count}");
        Console.WriteLine($"Expected:        {expectedCount}");
        Console.WriteLine($"Time Elapsed:    {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Threading:       {(useMulti ? "Multi-Threaded" : "Single-Threaded")}");
        Console.ResetColor();

        if (results.Count == expectedCount)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✅ Correct! All expected URLs found.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠️  Expected {expectedCount} URLs, but found {results.Count}");
            Console.ResetColor();
        }
    }

    static async Task RunBenchmark()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("Performance Benchmark");
        Console.WriteLine(new string('═', 60));

        Console.Write("Select test scenario (1=Example1, 2=Example2): ");
        var scenario = Console.ReadLine();

        string startUrl;
        IHtmlParser parser;
        int expectedCount;

        if (scenario == "2")
        {
            startUrl = "http://news.google.com";
            parser = MockHtmlParser.CreateExample2();
            expectedCount = 1;
        }
        else
        {
            startUrl = "http://news.yahoo.com/news/topics/";
            parser = MockHtmlParser.CreateExample1();
            expectedCount = 4;
        }

        Console.WriteLine($"\nBenchmarking: {startUrl}");
        Console.WriteLine("Running both single-threaded and multi-threaded...\n");

        // Single-threaded
        var singleCrawler = new SingleThreadedCrawler();
        var swSingle = Stopwatch.StartNew();
        var resultsSingle = await singleCrawler.CrawlAsync(startUrl, parser);
        swSingle.Stop();

        // Multi-threaded
        var multiCrawler = new MultiThreadedCrawler(maxConcurrency: 10);
        var swMulti = Stopwatch.StartNew();
        var resultsMulti = await multiCrawler.CrawlAsync(startUrl, parser);
        swMulti.Stop();

        // Results
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("Benchmark Results:");
        Console.WriteLine(new string('─', 60));
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"URLs Crawled:           {resultsSingle.Count}");
        Console.WriteLine($"Expected URLs:          {expectedCount}");
        Console.ResetColor();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Single-Threaded Time:   {swSingle.ElapsedMilliseconds}ms");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Multi-Threaded Time:    {swMulti.ElapsedMilliseconds}ms");
        Console.ResetColor();
        
        var speedup = (double)swSingle.ElapsedMilliseconds / swMulti.ElapsedMilliseconds;
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Speedup Factor:         {speedup:F2}x faster ⚡");
        Console.ResetColor();

        if (resultsSingle.Count == expectedCount && resultsMulti.Count == expectedCount)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✅ Both implementations are correct!");
            Console.ResetColor();
        }
    }

    static async Task RunCustomCrawl()
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("Custom Crawl (Using Example 1 Graph)");
        Console.WriteLine(new string('═', 60));
        
        Console.WriteLine("\nAvailable URLs in test graph:");
        Console.WriteLine("  - http://news.yahoo.com/news/topics/");
        Console.WriteLine("  - http://news.yahoo.com");
        Console.WriteLine("  - http://news.yahoo.com/news");
        Console.WriteLine("  - http://news.yahoo.com/us");
        Console.WriteLine("  - http://news.google.com");
        
        Console.Write("\nEnter start URL: ");
        var startUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(startUrl))
        {
            Console.WriteLine("Invalid URL!");
            return;
        }

        var parser = MockHtmlParser.CreateExample1();
        
        await RunCrawl(startUrl, parser, "Custom", expectedCount: -1);
    }
}
