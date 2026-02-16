# Coffee API 🚀

A modern, production-ready RESTful API for coffee management built with .NET 8 following Clean Architecture principles and industry best practices.

## 🏗️ <!--  -->Architecture

The solution follows Clean Architecture with clear separation of concerns:

```
src/
├── Coffee.Api/              # API Layer - Controllers & Middleware
├── Coffee.Application/      # Business Logic, DTOs, Services & Validators
├── Coffee.Domain/           # Domain Entities & Common Types
├── Coffee.Persistence/      # Data Access, EF Core & Repositories
└── Coffee.Tests/            # Unit & Integration Tests
```

### Design Patterns Implemented

- **Clean Architecture**: Dependency direction from outer to inner layers
- **Repository Pattern**: Abstraction for data access
- **Unit of Work**: Transaction coordination across multiple repositories
- **Service Layer**: Business logic encapsulation
- **Result Pattern**: Type-safe operation results without exceptions
- **CQRS-ready**: Separated read/write operations preparation
- **Options Pattern**: Type-safe configuration management

## ✨ Implemented Features

### 🔐 Security & Authentication
- **JWT Authentication**: Configured JWT tokens with validation
- **CORS**: Proper configuration for different origins
- **Security by Layers**: DTOs separated from entities, no direct exposure
- **Input Validation**: FluentValidation with comprehensive rules

### 📊 Data Management
- **Entity Framework Core**: Modern ORM with MySQL
- **Dapper-ready**: Template for high-performance queries
- **Migrations**: Database versioning control
- **Audit Fields**: Automatic CreatedAt/UpdatedAt tracking
- **Soft Delete**: Optional soft delete implementation
- **Entity Configurations**: Separate configuration classes

### 🚀 Performance & Scalability
- **Multi-level Caching**: Memory cache + Redis (configurable)
- **AsNoTracking**: Read-only query optimization
- **CancellationToken**: Full async/await with cancellation support
- **Pagination**: Complete support with total count in single query
- **Search & Filtering**: Name search with case-insensitive matching
- **Background Jobs**: Hangfire support (configurable)

### 📋 Quality & Testing
- **Unit Tests**: xUnit + Moq for business logic
- **Integration Tests**: WebApplicationFactory for API testing
- **FluentValidation**: Comprehensive input validation
- **Global Error Handling**: Middleware with standardized responses
- **Structured Logging**: Serilog with multiple sinks

### 📚 Documentation & Monitoring
- **Swagger/OpenAPI**: Interactive documentation with JWT support
- **Health Checks**: `/health` endpoint for monitoring
- **API Versioning**: Support for API versions
- **XML Documentation**: Controller and model documentation

### 🐳 Docker & Deployment
- **Dockerfile**: Optimized for production
- **Docker Compose**: Complete setup with MySQL and Redis
- **Multi-environment**: Separate configurations (dev/prod)

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- MySQL 8.0+
- Docker & Docker Compose (optional)

### Local Development

1. **Clone the repository**
```bash
git clone <repository-url>
cd NetExample
```

2. **Configure database**
```bash
# Create database and user
mysql -u root -p
CREATE DATABASE CoffeeDb;
CREATE USER 'coffeeuser'@'localhost' IDENTIFIED BY 'coffeepass';
GRANT ALL PRIVILEGES ON CoffeeDb.* TO 'coffeeuser'@'localhost';
FLUSH PRIVILEGES;
```

3. **Run migrations**
```bash
dotnet ef database update --project src/Coffee.Persistence --startup-project src/Coffee.Api
```

4. **Start the API**
```bash
dotnet run --project src/Coffee.Api
```

### Docker Execution

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f coffee-api
```

## 📡 Main Endpoints

### Coffee
- `GET /api/v1.0/coffee` - List coffees (with pagination, search & sorting)
- `GET /api/v1.0/coffee/{id}` - Get coffee by ID
- `POST /api/v1.0/coffee` - Create new coffee
- `PUT /api/v1.0/coffee/{id}` - Update coffee
- `DELETE /api/v1.0/coffee/{id}` - Delete coffee

### Health & Monitoring
- `GET /health` - Health check endpoint

### Query Examples

```bash
# Pagination
GET /api/v1.0/coffee?page=2&pageSize=5

# Search
GET /api/v1.0/coffee?search=espresso

# Sorting
GET /api/v1.0/coffee?sortBy=name&sortDescending=true

# Combined
GET /api/v1.0/coffee?search=latte&page=1&pageSize=10&sortBy=createdat
```

## 🔧 Configuration

### Environment Variables

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CoffeeDb;User=coffeeuser;Password=coffeepass;",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Issuer": "CoffeeApi",
    "Audience": "CoffeeApiUsers",
    "SecretKey": "your-secret-key-min-32-characters-long",
    "ExpirationMinutes": 60
  },
  "Cache": {
    "UseRedis": false,
    "DefaultExpiry": "00:30:00",
    "SlidingExpiry": "00:05:00",
    "RedisConnection": "localhost:6379",
    "RedisInstanceName": "Coffee_"
  }
}
```

### Cache Configuration

The application supports two caching strategies:

1. **Memory Cache** (default): In-process caching for single-instance deployments
2. **Redis**: Distributed caching for multi-instance deployments

Switch between them using the `Cache:UseRedis` configuration setting.

## 🧪 Testing

### Run Tests
```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test src/Coffee.Tests
```

### Test Structure
- **Unit Tests**: Business logic validation with mocked dependencies
- **Integration Tests**: Full request/response cycle with in-memory database

## 📊 Monitoring

### Health Check
```bash
curl http://localhost:8080/health
```

### Swagger UI
- Development: `http://localhost:5000`
- Swagger JSON: `http://localhost:5000/swagger/v1/swagger.json`

## 🛡️ Security

- JWT authentication with configurable tokens
- Input validation with FluentValidation
- CORS configured for production
- Security headers
- Structured logging for audit trails
- No secrets in code (use environment variables)

## 📈 Performance Optimizations

- **AsNoTracking**: For read-only queries to reduce memory overhead
- **CancellationToken**: All async operations support cancellation
- **Multi-level Caching**: L1 (Memory) + L2 (Redis) cache strategy
- **Pagination**: Server-side pagination with single-query total count
- **Entity Configuration**: Optimized indexes and constraints
- **Result Pattern**: Avoids exception-based flow control

## 📝 Logging

Structured logging with Serilog:
- **Console**: Structured output for development
- **File**: Daily rotation with 7-day retention
- **Levels**: Configurable per environment (Debug/Info/Error)

Log entries include:
- Timestamp
- Log level
- Message
- Exception details (when applicable)
- Custom properties (Application, RequestId, etc.)

## 🚨 Error Handling

Global middleware provides standardized error responses:

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "Coffee name is required",
  "traceId": "00-bf247f33cc130ab0f8f5b470327bfc6c",
  "instance": "/api/v1.0/coffee"
}
```

Error types handled:
- Validation errors (400)
- Not found (404)
- Duplicate entries (409)
- Unauthorized (401)
- Internal server errors (500)

## 🏗️ Code Structure

### Result Pattern
All service operations return a `Result<T>` or `Result`:

```csharp
public async Task<Result<CoffeeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    var coffee = await _repository.GetByIdAsync(id, ct);
    if (coffee == null)
        return Result<CoffeeDto>.Failure($"Coffee '{id}' not found", "NOT_FOUND");
    
    return Result<CoffeeDto>.Success(_mapper.Map<CoffeeDto>(coffee));
}
```

### Service Layer
Business logic is encapsulated in services:

```csharp
public class CoffeeService : ICoffeeService
{
    private readonly ICoffeeRepository _repository;
    private readonly ICacheService _cache;
    private readonly IValidator<CreateCoffeeDto> _validator;
    // ...
}
```

### Validation
FluentValidation provides comprehensive validation:

```csharp
public class CreateCoffeeDtoValidator : AbstractValidator<CreateCoffeeDto>
{
    public CreateCoffeeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

### Code Standards
- Follow existing code style
- Add unit tests for new features
- Update documentation
- Ensure all tests pass
- Use meaningful commit messages

## 📄 License

This project is licensed under the MIT License - see LICENSE file for details.

## 🙏 Acknowledgments

- .NET Team for the amazing framework
- Microsoft for Entity Framework Core
- Serilog team for excellent logging
- FluentValidation team for comprehensive validation
- Hangfire team for background job solution

## 📚 Additional Resources

### Patterns & Practices
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Result Pattern](https://github.com/altmann/FluentResults)
- [Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)

### Tools & Libraries
- [FluentValidation](https://fluentvalidation.net/)
- [AutoMapper](https://automapper.org/)
- [Serilog](https://serilog.net/)
- [xUnit](https://xunit.net/)

---

**Ready for the next coffee? ☕**

Built with ❤️ following .NET Backend Development Patterns
