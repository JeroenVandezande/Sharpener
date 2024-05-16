using System.Text.RegularExpressions;

namespace Sharpener.OpenAI;

internal class APIRate
{
    public DateTime TimeOfTransmission { get; set; }
    public int NumberOfTokes { get; set; }
}

public static class RateLimiter
{
    private static int _maxRate = 30000;
    private static Queue<APIRate> _rates = new Queue<APIRate>(); 
    private static List<string> _Tokenize(string input)
    {
        var tokens = new List<string>();
        var tokenMatches = Regex.Matches(input, @"\w+|[^\w\s]");
        foreach (Match match in tokenMatches)
        {
            tokens.Add(match.Value);
        }
        return tokens;
    }
    
    private static int _GetTokenCount(string input)
    {
        return _Tokenize(input).Count;
    }
    
    public static void WaitIfNeeded(string apiRequest)
    {
        var r = new APIRate();
        r.NumberOfTokes = _GetTokenCount(apiRequest);
        r.TimeOfTransmission = DateTime.Now;
        _rates.Enqueue(r);
        var totalTokensUsed = _maxRate + 1;
        while (totalTokensUsed >= _maxRate)
        {
            while (_rates.Count > 0 && (DateTime.Now - _rates.Peek().TimeOfTransmission).TotalMilliseconds > 1000)
            {
                _rates.Dequeue();
            }

            totalTokensUsed = 0;
            foreach (var rate in _rates)
            {
                totalTokensUsed += rate.NumberOfTokes;
            }
            Thread.Sleep(100);
        }
    }
}