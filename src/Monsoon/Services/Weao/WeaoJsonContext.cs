using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monsoon.Services.Weao
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReadCommentHandling = JsonCommentHandling.Skip)]
    [JsonSerializable(typeof(ExploitStatus))]
    [JsonSerializable(typeof(List<ExploitStatus>))]
    [JsonSerializable(typeof(RobloxVersions))]
    [JsonSerializable(typeof(SuncData))]
    [JsonSerializable(typeof(WeaoErrorResponse))]
    internal sealed partial class WeaoJsonContext : JsonSerializerContext
    {
    }
}
