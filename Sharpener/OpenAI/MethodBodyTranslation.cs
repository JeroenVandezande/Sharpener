using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Sharpener.OpenAI;

public static class MethodBodyTranslation
{
    private static OpenAIAPI _API = new OpenAIAPI(new APIAuthentication(KeyContainer.APIKey));
    private static Conversation _chat = _API.Chat.CreateConversation();

    static MethodBodyTranslation()
    {
        _chat.Model = Model.GPT4_32k_Context;
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
        string result = String.Empty;
        _chat.AppendUserInput(oxygeneCode);
        result = _chat.GetResponseFromChatbotAsync().Result;
        return result;
    }
}