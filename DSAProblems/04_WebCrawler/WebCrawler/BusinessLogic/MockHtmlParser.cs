using WebCrawler.Interfaces;

namespace WebCrawler.BusinessLogic;

/// <summary>
/// Mock HTML parser for testing purposes.
/// Simulates the blocking HTTP call with Thread.Sleep.
/// Contains pre-defined test scenarios from LeetCode examples.
/// </summary>
public class MockHtmlParser : IHtmlParser
{
    private readonly Dictionary<string, List<string>> _urlGraph;
    private readonly int _simulatedDelayMs;

    /// <summary>
    /// Creates a mock parser with a custom URL graph
    /// </summary>
    public MockHtmlParser(Dictionary<string, List<string>> urlGraph, int simulatedDelayMs = 15)
    {
        _urlGraph = urlGraph;
        _simulatedDelayMs = simulatedDelayMs;
    }

    /// <summary>
    /// Creates a mock parser with a predefined test scenario
    /// </summary>
    public static MockHtmlParser CreateFromScenario(string scenario)
    {
        return scenario.ToLower() switch
        {
            "example1" => CreateExample1(),
            "example2" => CreateExample2(),
            _ => throw new ArgumentException($"Unknown scenario: {scenario}")
        };
    }

    /// <summary>
    /// Example 1 from LeetCode:
    /// Start: http://news.yahoo.com/news/topics/
    /// Expected: 4 yahoo.com URLs
    /// </summary>
    public static MockHtmlParser CreateExample1()
    {
        var graph = new Dictionary<string, List<string>>
        {
            ["http://news.yahoo.com/news/topics/"] = new()
            {
                "http://news.yahoo.com",
                "http://news.yahoo.com/news"
            },
            ["http://news.yahoo.com"] = new()
            {
                "http://news.yahoo.com/us"
            },
            ["http://news.yahoo.com/news"] = new()
            {
                "http://news.google.com",  // Different hostname - should be filtered
                "http://news.yahoo.com/news/topics/"
            },
            ["http://news.google.com"] = new()
            {
                "http://news.yahoo.com"
            },
            ["http://news.yahoo.com/us"] = new List<string>()  // No outgoing links
        };

        return new MockHtmlParser(graph);
    }

    /// <summary>
    /// Example 2 from LeetCode:
    /// Start: http://news.google.com
    /// Expected: Only the start URL (all links go to different hostnames)
    /// </summary>
    public static MockHtmlParser CreateExample2()
    {
        var graph = new Dictionary<string, List<string>>
        {
            ["http://news.google.com"] = new()
            {
                "http://news.yahoo.com",
                "http://news.yahoo.com/news"
            },
            ["http://news.yahoo.com"] = new()
            {
                "http://news.google.com",
                "http://news.yahoo.com/news/topics/"
            },
            ["http://news.yahoo.com/news/topics/"] = new()
            {
                "http://news.yahoo.com",
                "http://news.yahoo.com/news"
            },
            ["http://news.yahoo.com/news"] = new()
            {
                "http://news.google.com",
                "http://news.yahoo.com/news/topics/"
            }
        };

        return new MockHtmlParser(graph);
    }

    public List<string> GetUrls(string url)
    {
        // Simulate blocking HTTP call
        Thread.Sleep(_simulatedDelayMs);

        // Return URLs from the graph, or empty list if URL not in graph
        return _urlGraph.TryGetValue(url, out var urls) ? urls : new List<string>();
    }
}
