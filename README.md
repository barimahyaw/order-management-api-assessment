# Order Management API Assessment

## Overview
Simple, clean Order Management API built with .NET 8 demonstrating core business features with minimal complexity. Implements a traditional MVC architecture pattern focusing on maintainability and clear separation of concerns.

## Features Implemented

### ✅ **Discounting System**
- **First-time buyer discount**: 10% off for new customers
- **Bulk order discount**: 15% off for orders with 10+ items  
- **VIP customer discount**: 20% off for VIP segment customers
- **Automatic best discount selection**: System chooses highest applicable discount

### ✅ **Order Status Tracking**
- **State transitions**: Pending → Processing → Shipped → Delivered
- **Business rules**: Cancellation only from Pending/Processing states
- **Validation**: Prevents invalid status transitions
- **Fulfillment tracking**: Automatic timestamp when delivered

### ✅ **Order Analytics Endpoint**
- **Key metrics**: Total orders, revenue, average order value
- **Status distribution**: Orders grouped by current status
- **Performance optimized**: Database-level calculations

### ✅ **Complete CRUD Operations**
- Create orders with automatic discount application
- Retrieve individual orders with full details
- Update order status with validation
- List orders with pagination and status filtering

### ✅ **Comprehensive Testing**
- **Unit tests**: Business logic validation (DiscountService, Models)
- **Integration tests**: Full API endpoint testing with real database
- **Test helpers**: Reusable utilities for clean test code
- **Coverage**: All discount scenarios, status transitions, API endpoints

## Architecture

**Clean layered architecture:**
- **Controllers** → Handle HTTP requests/responses
- **Services** → Business logic and orchestration  
- **Data** → Entity Framework Core with proper modeling
- **Models** → Domain entities with business rules

**Key patterns:**
- Dependency injection for loose coupling
- Service layer for business logic isolation
- Repository pattern via Entity Framework DbContext
- Domain modeling with encapsulated business rules

## Technology Stack

- **.NET 8** - Modern framework with latest features
- **Entity Framework Core** - ORM with InMemory database
- **Swagger/OpenAPI** - Automatic API documentation
- **xUnit + FluentAssertions** - Testing framework
- **ASP.NET Core MVC** - Web API framework

## Quick Start

```bash
# Clone and run
git clone <repository-url>
cd order-management-api-assessment/order-management-api-assessment
dotnet run

# Access Swagger UI
https://localhost:7000/swagger
```

## API Endpoints

```
POST   /api/orders                    - Create new order
GET    /api/orders                    - Get orders (paginated, filterable by status)
GET    /api/orders/{orderId}          - Get specific order details
PUT    /api/orders/{orderId}/status   - Update order status
GET    /api/orders/analytics          - Get order analytics
```

## Sample Usage

### Create Order
```json
POST /api/orders
{
  "customerId": "2585a176-1e69-4d3c-b174-9da5f5521505",
  "orderItems": [
    {
      "productId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
      "quantity": 2,
      "unitPrice": 99.99
    }
  ]
}
```

### Update Status
```json
PUT /api/orders/{orderId}/status
{
  "newStatus": 2  // Processing
}
```

## Performance Optimizations

- **Database indexing**: CustomerId, Status, CreatedAt for efficient queries
- **Pagination**: Prevents large data sets (max 100 items per page)
- **Async operations**: All database calls use async/await
- **Efficient projections**: Select only required fields for list operations

## Testing Strategy

**Multi-level testing approach:**
- **Unit tests**: Isolated business logic testing (DiscountService)
- **Service tests**: Business layer with real database (InMemory)
- **Integration tests**: Full HTTP stack testing with WebApplicationFactory
- **Test utilities**: Shared helpers reduce duplication and improve readability

## Key Assumptions

1. **InMemory database**: Used for demo/assessment - easily switchable to SQL Server
2. **Mock Product IDs**: Product management not in scope, using placeholder GUIDs
3. **No authentication**: Not specified in requirements
4. **Best discount auto-selection**: System automatically applies highest available discount
5. **Seeded test data**: Pre-loaded customers and orders for demo purposes

## Solution Highlights

- **Clean, readable code**: Focused on simplicity and maintainability
- **All requirements met**: Discounts, status tracking, analytics, testing
- **Production considerations**: Proper error handling, logging, validation
- **Extensible design**: Easy to add new discount rules or order features
- **Performance ready**: Proper indexing and query optimization

This solution demonstrates practical .NET development skills with focus on clean code, proper testing, and business requirement implementation.
