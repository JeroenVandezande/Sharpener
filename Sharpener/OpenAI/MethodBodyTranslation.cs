using System.Diagnostics;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Sharpener.OpenAI;

public static class MethodBodyTranslation
{
    private static OpenAIAPI _API = new OpenAIAPI(new APIAuthentication(KeyContainer.APIKey));
    private static Conversation _chat = _API.Chat.CreateConversation();
    private static Stopwatch _stopWatch = new Stopwatch();
    private static readonly int _RateLimit = 500;
    private static readonly int _RateDelayInms = (int)(1000.0 / _RateLimit) + 2; //+2 for safety
    
    public static bool SkipOpenAICalls { get; set; }

    static MethodBodyTranslation()
    {
        _stopWatch.Start();
        var model =  new Model("gpt-4o") { OwnedBy = "openai" };
        _chat.Model = model;
        _chat.RequestParameters.Temperature = 0;
        _chat.AppendSystemMessage(@"You are a master in translating code from Remobjects Oxygene to C#.
        Refrain from explaining, do not say anything else.
        Do not add a code-block around the resulting code.
        Translate the given Oxygene method to C#.
        Only return the method body, NOT the method definition.
        Make sure to include the code comments, e.g. //Some comment");
    }
    
    public static string TranslateOxygeneToCS(string oxygeneCode)
    {
        if (SkipOpenAICalls) return "/* " + oxygeneCode + " */";
        while (_stopWatch.ElapsedMilliseconds < _RateDelayInms)
        {
            Thread.Sleep(5);
        }
        string result = String.Empty;
        _chat.AppendUserInput(oxygeneCode);
        result = _chat.GetResponseFromChatbotAsync().Result;
        _stopWatch.Restart();
        return result;
    }
}