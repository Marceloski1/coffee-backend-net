using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Coffee.Persistence.Data;
using Coffee.Domain.Entities;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Coffee.Tests.Integration;

public class ControllersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly CoffeeDbContext _context;

    public ControllersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CoffeeDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<CoffeeDbContext>(options =>
                {
                    options.UseInMemoryDatabase("CoffeeTestDb");
                });

                // Create the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    _context = scopedServices.GetRequiredService<CoffeeDbContext>();

                    // Ensure the database is created
                    _context.Database.EnsureCreated();

                    // Seed test data
                    SeedTestData();
                }
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _context.Coffees.RemoveRange(_context.Coffees);
        _context.Categories.RemoveRange(_context.Categories);
        _context.SaveChanges();

        // Add test categories
        var categories = new List<Category>
        {
            new Category { Name = "Espresso" },
            new Category { Name = "Cappuccino" }
        };
        _context.Categories.AddRange(categories);
        _context.SaveChanges();

        // Add test coffees
        var coffees = new List<Coffee>
        {
            new Coffee { Name = "Americano", CategoryId = categories[0].Id },
            new Coffee { Name = "Latte", CategoryId = categories[1].Id }
        };
        _context.Coffees.AddRange(coffees);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCategories_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await _client.GetAsync("/api/v1.0/category");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task GetCategories_ReturnsListOfCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/v1.0/category");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var categories = JsonConvert.DeserializeObject<List<dynamic>>(content);
        Assert.True(categories.Count >= 2);
    }

    [Fact]
    public async Task CreateCategory_ValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var categoryData = new { Name = "Mocha" };
        var content = JsonConvert.SerializeObject(categoryData);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1.0/category", stringContent);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var createdContent = await response.Content.ReadAsStringAsync();
        var createdCategory = JsonConvert.DeserializeObject<dynamic>(createdContent);
        Assert.Equal("Mocha", createdCategory.name.ToString());
    }

    [Fact]
    public async Task CreateCategory_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var categoryData = new { Name = "" }; // Invalid: empty name
        var content = JsonConvert.SerializeObject(categoryData);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1.0/category", stringContent);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCoffees_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/v1.0/coffee?page=1&pageSize=1");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonConvert.DeserializeObject<dynamic>(content);
        
        Assert.Equal(1, pagedResult.pageSize.Value);
        Assert.Equal(1, pagedResult.page.Value);
        Assert.True(pagedResult.items.Count <= 1);
    }

    [Fact]
    public async Task GetCoffees_WithSearch_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1.0/coffee?search=Americano");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonConvert.DeserializeObject<dynamic>(content);
        
        Assert.True(pagedResult.items.Count >= 1);
        Assert.Equal("Americano", pagedResult.items[0].name.ToString());
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
    }
}