using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sharpener.Enums;


[JsonConverter(typeof(StringEnumConverter))]  
public enum VisibilityLevel
{
    Public,
    Assembly,
    Protected,
    Private,
    None
};
