using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Coffee.Application.Interfaces;
using Coffee.Domain.Entities;
using Coffee.Application.DTOs;
using AutoMapper;
using Coffee.Api.Controllers;

namespace Coffee.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _mockMapper = new Mock<IMapper>();
        _controller = new CategoryController(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Espresso" },
            new Category { Id = Guid.NewGuid(), Name = "Cappuccino" }
        };

        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Id = categories[0].Id, Name = "Espresso", CoffeeCount = 0 },
            new CategoryDto { Id = categories[1].Id, Name = "Cappuccino", CoffeeCount = 0 }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockMapper.Setup(m => m.Map<IEnumerable<CategoryDto>>(categories)).Returns(categoryDtos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Equal(2, returnedCategories.Count());
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        // Act
        var result = await _controller.GetById(categoryId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Espresso" };
        var categoryDto = new CategoryDto { Id = category.Id, Name = "Espresso", CoffeeCount = 0 };

        _mockRepository.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);
        _mockMapper.Setup(m => m.Map<CategoryDto>(category)).Returns(categoryDto);

        // Act
        var result = await _controller.GetById(category.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(category.Id, returnedCategory.Id);
        Assert.Equal(category.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Espresso" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Espresso" };
        var categoryDto = new CategoryDto { Id = category.Id, Name = "Espresso", CoffeeCount = 0 };

        _mockMapper.Setup(m => m.Map<Category>(createDto)).Returns(category);
        _mockRepository.Setup(r => r.CreateAsync(category)).ReturnsAsync(category);
        _mockMapper.Setup(m => m.Map<CategoryDto>(category)).Returns(categoryDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(createdAtResult.Value);
        Assert.Equal(category.Id, returnedCategory.Id);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Espresso Updated" };

        _mockRepository.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        // Act
        var result = await _controller.Update(categoryId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(categoryId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(categoryId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(categoryId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(categoryId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}