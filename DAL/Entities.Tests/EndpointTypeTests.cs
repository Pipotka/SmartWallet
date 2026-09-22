using FluentAssertions;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Entities.Tests;

public class EndpointTypeTests
{
    [Fact]
    public void EndpointType_ShouldHaveExpectedIntegerValues()
    {
        ((int)EndpointType.Storage).Should().Be(0);
        ((int)EndpointType.Category).Should().Be(1);
    }
}
