using TeamX.Security.Licensing;

namespace TeamX.Tests.Licensing;

public class LicenseKeyGeneratorTests
{
    [Fact]
    public void ShouldGenerateValidLicenseKey()
    {
        var generator = new LicenseKeyGenerator();

        var key = generator.Generate();


        Assert.NotNull(key);

        Assert.True(
            generator.IsValidFormat(key)
        );
    }
}