using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrdersApi.Controllers;
using OrdersApi.Data;
using OrdersApi.Dtos;
using OrdersApi.Models;
using Xunit;

namespace OrdersApi.Tests
{
    public class OrdersControllerTests
    {
        private readonly OrdersDbContext _db;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _db = TestHelpers.GetInMemoryDbContext();
            var logger = new LoggerFactory().CreateLogger<OrdersController>();
            _controller = new OrdersController(_db, logger);
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ReturnsCreatedOrder()
        {
            var dto = new CreateOrderDto
            {
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductName = "Test Product", Quantity = 2, UnitPrice = 10 }
                }
            };

            var result = await _controller.CreateOrder(dto) as CreatedAtActionResult;
            var createdOrder = result?.Value as Order;

            Assert.NotNull(result);
            Assert.NotNull(createdOrder);
            Assert.Single(createdOrder.Items);
        }

        [Fact]
        public async Task CreateOrder_WithNoItems_ReturnsBadRequest()
        {
            var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto>() };

            var result = await _controller.CreateOrder(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_WithExistingOrder_ReturnsOrder()
        {
            // Arrange
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Test", Quantity = 1, UnitPrice = 5 }
                }
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrderById(order.Id) as OkObjectResult;
            var fetchedOrder = result?.Value as Order;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.Id, fetchedOrder?.Id);
        }

        [Fact]
        public async Task GetOrderById_WithNonExistingOrder_ReturnsNotFound()
        {
            var result = await _controller.GetOrderById(Guid.NewGuid()) as NotFoundResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_WithValidData_UpdatesOrder()
        {
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Old", Quantity = 1, UnitPrice = 5 }
                }
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var dto = new CreateOrderDto
            {
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductName = "Updated", Quantity = 2, UnitPrice = 10 }
                }
            };

            var result = await _controller.UpdateOrder(order.Id, dto) as OkObjectResult;
            var updatedOrder = result?.Value as Order;

            Assert.NotNull(result);
            Assert.Single(updatedOrder?.Items);
            Assert.Equal("Updated", updatedOrder?.Items.First().ProductName);
        }

        [Fact]
        public async Task UpdateOrder_WithNoItems_ReturnsBadRequest()
        {
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Old", Quantity = 1, UnitPrice = 5 }
                }
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto>() };

            var result = await _controller.UpdateOrder(order.Id, dto) as BadRequestObjectResult;
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_WithExistingOrder_DeletesOrder()
        {
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Test", Quantity = 1, UnitPrice = 5 }
                }
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var result = await _controller.DeleteOrder(order.Id) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
            Assert.Empty(_db.Orders);
        }

        [Fact]
        public async Task OrderNumber_ShouldIncrement_ForEachNewOrder()
        {
            var dto1 = new CreateOrderDto
            {
                Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductName = "A", Quantity = 1, UnitPrice = 1 } }
            };
            var dto2 = new CreateOrderDto
            {
                Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductName = "B", Quantity = 1, UnitPrice = 1 } }
            };

            var result1 = await _controller.CreateOrder(dto1) as CreatedAtActionResult;
            var result2 = await _controller.CreateOrder(dto2) as CreatedAtActionResult;

            var order1 = result1?.Value as Order;
            var order2 = result2?.Value as Order;

            Assert.NotNull(order1);
            Assert.NotNull(order2);
            Assert.True(order2!.OrderNumber > order1!.OrderNumber);
        }
    }
}
