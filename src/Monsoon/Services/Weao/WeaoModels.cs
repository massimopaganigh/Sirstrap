using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monsoon.Services.Weao
{
    public sealed class RobloxVersions
    {
        [JsonPropertyName("Android")]
        public string? Android { get; set; }

        [JsonPropertyName("AndroidDate")]
        public string? AndroidDate { get; set; }

        [JsonPropertyName("iOS")]
        public string? IOS { get; set; }

        [JsonPropertyName("iOSDate")]
        public string? IOSDate { get; set; }

        [JsonPropertyName("Mac")]
        public string? Mac { get; set; }

        [JsonPropertyName("MacDate")]
        public string? MacDate { get; set; }

        [JsonPropertyName("MacResponse")]
        public JsonElement? MacResponse { get; set; }

        [JsonPropertyName("Windows")]
        public string? Windows { get; set; }

        [JsonPropertyName("WindowsDate")]
        public string? WindowsDate { get; set; }

        [JsonPropertyName("WindowsResponse")]
        public JsonElement? WindowsResponse { get; set; }
    }

    public sealed class ExploitStatus
    {
        [JsonPropertyName("beta")]
        public bool Beta { get; set; }

        [JsonPropertyName("clientmods")]
        public bool ClientMods { get; set; }

        [JsonPropertyName("cost")]
        public string? Cost { get; set; }

        [JsonPropertyName("decompiler")]
        public bool Decompiler { get; set; }

        [JsonPropertyName("detected")]
        public bool Detected { get; set; }

        [JsonPropertyName("discordlink")]
        public string? DiscordLink { get; set; }

        [JsonPropertyName("extype")]
        public string? ExType { get; set; }

        [JsonPropertyName("free")]
        public bool Free { get; set; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }

        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("keysystem")]
        public bool KeySystem { get; set; }

        [JsonPropertyName("multiInject")]
        public bool MultiInject { get; set; }

        [JsonPropertyName("platform")]
        public string? Platform { get; set; }

        [JsonPropertyName("purchaselink")]
        public string? PurchaseLink { get; set; }

        [JsonPropertyName("raknet")]
        public bool Raknet { get; set; }

        [JsonPropertyName("rbxversion")]
        public string? RbxVersion { get; set; }

        [JsonPropertyName("sunc")]
        public SuncCredentials? Sunc { get; set; }

        [JsonPropertyName("suncPercentage")]
        public int SuncPercentage { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("uncPercentage")]
        public int UncPercentage { get; set; }

        [JsonPropertyName("uncStatus")]
        public bool UncStatus { get; set; }

        [JsonPropertyName("updatedDate")]
        public string? UpdatedDate { get; set; }

        [JsonPropertyName("updateStatus")]
        public bool UpdateStatus { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("websitelink")]
        public string? WebsiteLink { get; set; }
    }

    public sealed class SuncCredentials
    {
        [JsonPropertyName("suncKey")]
        public string? Key { get; set; }

        [JsonPropertyName("suncScrap")]
        public string? Scrap { get; set; }
    }

    public sealed class SuncData
    {
        [JsonPropertyName("bibip")]
        public bool Bibip { get; set; }

        [JsonPropertyName("executor")]
        public string? Executor { get; set; }

        [JsonPropertyName("outdated")]
        public bool Outdated { get; set; }

        [JsonPropertyName("tests")]
        public SuncTests? Tests { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }

        [JsonPropertyName("timeTaken")]
        public double TimeTaken { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    public sealed class SuncTests
    {
        [JsonPropertyName("failed")]
        public List<SuncTest> Failed { get; set; } = [];

        [JsonPropertyName("passed")]
        public List<SuncTest> Passed { get; set; } = [];
    }

    public sealed class SuncTest
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("library")]
        public string? Library { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public sealed class WeaoErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("rateLimitInfo")]
        public RateLimitInfo? RateLimitInfo { get; set; }
    }

    public sealed class RateLimitInfo
    {
        [JsonPropertyName("remainingTime")]
        public int RemainingTime { get; set; }

        [JsonPropertyName("requestsRemaining")]
        public int RequestsRemaining { get; set; }

        [JsonPropertyName("resetTime")]
        public long ResetTime { get; set; }
    }
}
