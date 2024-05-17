using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Sharpener.OpenAI;

internal class APIRate
{
    public DateTime TimeOfTransmission { get; set; }
    public int NumberOfTokens { get; set; }
}

public class RateLimiter
{
    private readonly ConcurrentQueue<APIRate> _tokenUsageQueue = new ConcurrentQueue<APIRate>();

    public int CombinedRateLimit { get; set; }  // Combined max tokens per minute
    
    
    public  void WaitIfNeeded(int completionTokens)
    {
        
        // Track the tokens used by this request and response
        _tokenUsageQueue.Enqueue(new APIRate { NumberOfTokens = completionTokens, TimeOfTransmission = DateTime.Now });
        LogState("After Enqueue");

        CleanUpOldEntries();

        var totalTokensUsed = _tokenUsageQueue.Sum(rate => rate.NumberOfTokens);

        // Wait if the total tokens used exceed the rate limit
        while (totalTokensUsed >= CombinedRateLimit)
        {
            Thread.Sleep(100);
            CleanUpOldEntries();
            totalTokensUsed = _tokenUsageQueue.Sum(rate => rate.NumberOfTokens);
            LogState($"During Waiting: TotalTokensUsed={totalTokensUsed}");
        }
    }
    
    private void CleanUpOldEntries()
    {
        while (_tokenUsageQueue.TryPeek(out var oldestEntry) && (DateTime.Now - oldestEntry.TimeOfTransmission).TotalMilliseconds > 60000)
        {
            _tokenUsageQueue.TryDequeue(out _);
        }
        LogState("After Cleanup");
    }

    private void LogState(string context)
    {
        Console.WriteLine($"{context}:");
        foreach (var rate in _tokenUsageQueue)
        {
            Console.WriteLine($"Time: {rate.TimeOfTransmission:HH:mm:ss.fff}, Tokens: {rate.NumberOfTokens}");
        }
        var totalTokensUsed = _tokenUsageQueue.Sum(rate => rate.NumberOfTokens);
        Console.WriteLine($"Total Tokens Used: {totalTokensUsed} / {CombinedRateLimit}");
    }

}