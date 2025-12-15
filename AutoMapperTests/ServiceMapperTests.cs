using AutoMapper;
using Nasurino.SmartWallet.Services.AutoMappers;

namespace AutoMapperTests;

/// <summary>
/// Тесты на маппер для сервисов
/// </summary>
public sealed class ServiceMapperTests
{
    /// <summary>
    /// Профайл маппера должен быть валидным
    /// </summary>
    [Fact]
    public void ProfileShouldBeValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ServiceModelMapper>());
        
        // Assert
        config.AssertConfigurationIsValid();
    }
}