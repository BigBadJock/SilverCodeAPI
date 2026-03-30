# SilverCodeAPI - Copilot Instructions

## Project Overview
This is a .NET 9 API solution providing common infrastructure for data access patterns, including repository pattern and unit of work implementations.

## Technology Stack
- **Target Framework**: .NET 9
- **Language**: C#
- **Architecture**: Repository pattern with data services
- **Key Components**:
  - Core.Common: Base implementations for repositories and data services
  - Core.Common.Contracts: Interfaces and contracts
  - Core.Common.DataModels: Data models and entities

## Coding Standards

### General Guidelines
- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use nullable reference types appropriately
- Prefer async/await for I/O operations
- Keep methods focused and single-purpose

### Architecture Patterns
- **Repository Pattern**: Use generic repository base classes (BaseRepository, BaseRepositoryWithIntId, BaseRepositoryWithGuidId, BaseRepositoryWithStringId)
- **Data Service Pattern**: Implement data services using BaseDataService and its variants
- **Unit of Work**: Use IUnitOfWork for transaction management
- **Database Factory**: Use IDatabaseFactory for database context creation

### Code Organization
- Interfaces go in Core.Common.Contracts
- Base implementations go in Core.Common
- Use generic type constraints for ID types (int, Guid, string)
- Separate read operations (IReadRepository) from write operations (IRepository)

### Dependency Injection
- Design all services and repositories for dependency injection
- Use constructor injection
- Register services with appropriate lifetimes

### Error Handling
- Use appropriate exception types
- Include meaningful error messages
- Consider adding try-catch blocks for external dependencies

### Testing
- Write unit tests for business logic
- Consider integration tests for repository implementations
- Mock dependencies appropriately

### Documentation
- Add XML documentation comments for public APIs
- Document complex logic with inline comments
- Keep README files updated

## Project-Specific Rules
- When creating new repositories, inherit from appropriate base classes
- When creating new data services, inherit from BaseDataService variants
- Maintain consistency with existing ID type patterns (int, Guid, string)
- Follow the established separation between contracts and implementations
- Use IAuditor for audit logging when applicable

## File Naming
- Interface files: I{Name}.cs (e.g., IRepository.cs)
- Base class files: Base{Name}.cs (e.g., BaseRepository.cs)
- Keep file names consistent with the class names they contain

## Common Tasks
- **Adding a new repository**: Inherit from BaseRepository or its typed variants, implement required interfaces
- **Adding a new data service**: Inherit from BaseDataService or its typed variants
- **Adding new contracts**: Place interfaces in Core.Common.Contracts project
- **Extending functionality**: Consider adding to base classes when logic is reusable across multiple implementations
