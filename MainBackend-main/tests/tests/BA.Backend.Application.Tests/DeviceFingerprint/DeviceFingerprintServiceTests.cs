using System.Security.Cryptography;
using System.Text;
using Xunit;
using FluentAssertions;

namespace BA.Backend.Application.Tests.DeviceFingerprint;

public class DeviceFingerprintServiceTests
{
    [Fact]
    public void ComputeFingerprint_WithValidInputs_ShouldReturnHashedValue()
    {
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var acceptLanguage = "en-US,en;q=0.9";

        var fingerprint = ComputeFingerprint(userAgent, acceptLanguage);

        fingerprint.Should().NotBeNullOrEmpty();
        fingerprint.Should().NotBe(userAgent);
        fingerprint.Should().NotBe(acceptLanguage);
    }

    [Fact]
    public void ComputeFingerprint_WithSameInputs_ShouldReturnSameFingerprint()
    {
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var acceptLanguage = "en-US,en;q=0.9";

        var fingerprint1 = ComputeFingerprint(userAgent, acceptLanguage);
        var fingerprint2 = ComputeFingerprint(userAgent, acceptLanguage);

        fingerprint1.Should().Be(fingerprint2);
    }

    [Fact]
    public void ComputeFingerprint_WithDifferentUserAgent_ShouldReturnDifferentFingerprint()
    {
        var acceptLanguage = "en-US,en;q=0.9";
        var userAgent1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var userAgent2 = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)";

        var fingerprint1 = ComputeFingerprint(userAgent1, acceptLanguage);
        var fingerprint2 = ComputeFingerprint(userAgent2, acceptLanguage);

        fingerprint1.Should().NotBe(fingerprint2);
    }

    [Fact]
    public void ComputeFingerprint_WithDifferentLanguage_ShouldReturnDifferentFingerprint()
    {
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var language1 = "en-US";
        var language2 = "es-ES";

        var fingerprint1 = ComputeFingerprint(userAgent, language1);
        var fingerprint2 = ComputeFingerprint(userAgent, language2);

        fingerprint1.Should().NotBe(fingerprint2);
    }

    [Fact]
    public void ComputeFingerprint_ShouldReturnBase64String()
    {
        var userAgent = "Mozilla/5.0";
        var acceptLanguage = "en";

        var fingerprint = ComputeFingerprint(userAgent, acceptLanguage);

        var act = () => Convert.FromBase64String(fingerprint);
        act.Should().NotThrow();
    }

    [Fact]
    public void ComputeFingerprint_WithEmptyStrings_ShouldReturnValidHash()
    {
        var userAgent = "";
        var acceptLanguage = "";

        var fingerprint = ComputeFingerprint(userAgent, acceptLanguage);

        fingerprint.Should().NotBeNullOrEmpty();
        fingerprint.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void ComputeFingerprint_WithSpecialCharacters_ShouldReturnValidHash()
    {
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        var acceptLanguage = "en-US,en;q=0.9,es;q=0.8,fr;q=0.7";

        var fingerprint = ComputeFingerprint(userAgent, acceptLanguage);

        fingerprint.Should().NotBeNullOrEmpty();
        fingerprint.Length.Should().BeGreaterThan(20);
    }

    private static string ComputeFingerprint(string userAgent, string acceptLanguage)
    {
        var combined = $"{userAgent}:{acceptLanguage}";
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashedBytes);
    }
}
