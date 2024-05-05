using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sharpener.Enums;

[JsonConverter(typeof(StringEnumConverter))]  
public enum CommentType
{
    SingleLineComment, SingleLineDocComment, MultilineComment
}