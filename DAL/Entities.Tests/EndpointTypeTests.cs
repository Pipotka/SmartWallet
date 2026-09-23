using FluentAssertions;
using Nasurino.SmartWallet.Entities.Enums;
using Xunit;

namespace Nasurino.SmartWallet.Entities.Tests;

public class EndpointTypeTests
{
    [Fact]
    public void EndpointType_ShouldHaveExpectedIntegerValues()
    {
        ((int)EndpointType.Category).Should().Be(0);
        ((int)EndpointType.Storage).Should().Be(1);
    }
}
