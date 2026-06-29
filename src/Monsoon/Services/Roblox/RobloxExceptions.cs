using System;
using System.Net;

namespace Monsoon.Services.Roblox
{
    public class RobloxDeploymentException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null) : Exception(message, innerException)
    {
        public HttpStatusCode? StatusCode { get; } = statusCode;
    }

    public sealed class RobloxManifestException(string message) : RobloxDeploymentException(message);

    public sealed class RobloxHashMismatchException(string packageName, string expected, string actual) : RobloxDeploymentException($"Hash mismatch for package '{packageName}': expected {expected}, got {actual}.")
    {
        public string Actual { get; } = actual;

        public string Expected { get; } = expected;

        public string PackageName { get; } = packageName;
    }
}
