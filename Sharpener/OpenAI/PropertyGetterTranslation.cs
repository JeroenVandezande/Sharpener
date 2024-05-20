using System.Diagnostics;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Sharpener.OpenAI;

public static class PropertyGetterTranslation
{
    private static OpenAIAPI _API = new OpenAIAPI(new APIAuthentication(KeyContainer.APIKey));
    private static Conversation _chat = _API.Chat.CreateConversation();
    private static RateLimiter _Limit = new RateLimiter();
   
    
    public static bool SkipOpenAICalls { get { return KeyContainer.SkipOpenAICalls; } }

    static PropertyGetterTranslation()
    {
        _Limit.CombinedRateLimit = 28000;
        var model =  new Model("gpt-4o") { OwnedBy = "openai" };
        _chat.Model = model;
        _chat.RequestParameters.Temperature = 0;
        var systemMessage = @"You are a master in translating code from Remobjects Oxygene to C#.
        Refrain from explaining, do not say anything else.
        Do not add a code-block around the resulting code.
        Translate the given Oxygene property getter to C# as an expression-body.
        e.g. 'IsMTPWell or IsPredilutionWell' becomes 'IsMTPWell || IsPredilutionWell'
        e.g. 'GetFullCaption' becomes 'GetFullCaption()'
        e.g. 'get_HoleCompatibility becomes 'get_HoleCompatibility()'";
        _chat.AppendSystemMessage(systemMessage);
    }
    
    public static string TranslateOxygeneToCS(string oxygeneCode)
    {
        if (KeyContainer.SkipOpenAICalls) return "/* " + oxygeneCode + " */";
       
        string result = String.Empty;
        _chat.AppendUserInput(oxygeneCode);
        result = _chat.GetResponseFromChatbotAsync().Result;
        _Limit.WaitIfNeeded( _chat.MostRecentApiResult.Usage.PromptTokens);
        return result;
        
    }
}