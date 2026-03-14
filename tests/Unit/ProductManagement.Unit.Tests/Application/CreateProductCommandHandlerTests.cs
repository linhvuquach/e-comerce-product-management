using FluentAssertions;
using MediatR;
using Moq;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Products.Commands.CreateProduct;
using ProductManagement.Application.Products.Events;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.ValueObjects;

namespace ProductManagement.Unit.Tests.Application;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISlugService> _slugService = new();
    private readonly Mock<IPublisher> _publisher = new();

    private CreateProductCommandHandler CreateHandler() =>
        new(_productRepo.Object, _unitOfWork.Object, _slugService.Object, _publisher.Object);

    private static CreateProductCommand DefaultCommand(
        string name = "Test Product",
        string brand = "Test Brand",
        decimal basePrice = 99.99m,
        string currency = "USD") =>
        new(name, brand, Guid.NewGuid(), basePrice, currency, null, null);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewProductId()
    {
        var command = DefaultCommand();
        var slug = Slug.Create("test-product");
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slug);

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsProductToRepository()
    {
        var command = DefaultCommand();
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("test-product"));

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        var command = DefaultCommand();
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("test-product"));

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesProductChangedNotification()
    {
        var command = DefaultCommand();
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("test-product"));

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _publisher.Verify(p => p.Publish(
            It.IsAny<ProductChangedNotification>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsProductWithCorrectName()
    {
        var command = DefaultCommand(name: "Nike Air Max");
        _slugService.Setup(s => s.GenerateUniqueSlugAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Slug.Create("nike-air-max"));

        Product? captured = null;
        _productRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured = p);

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Nike Air Max");
        captured.Brand.Should().Be(command.Brand);
        captured.CategoryId.Should().Be(command.CategoryId);
        captured.BasePrice.Should().Be(Money.Of(command.BasePrice, command.Currency));
    }
}
