using AutoMapper;
using Nasurino.SmartWallet.AutoMappers;

namespace Nasurino.SmartWallet.AutoMapper.Tests;

/// <summary>
/// Тесты на маппер для api 
/// </summary>
public sealed class ApiMapperTests
{
    /// <summary>
    /// Профайл маппера должен быть валидным
    /// </summary>
    [Fact]
    public void ProfileShouldBeValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ApiModelMapper>());
        
        // Assert
        config.AssertConfigurationIsValid();
    }
}