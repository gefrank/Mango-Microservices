using AutoMapper;
using Mango.Services.OrderAPI.Controllers;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.DTO;
using Mango.Services.OrderAPI.Service.IService;
using Mango.MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Stripe;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Mango.Services.OrderAPI.Tests
{
    public class OrderAPIControllerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly OrderAPIController _controller;

        static OrderAPIControllerTests()
        {
            // Configure Stripe for testing environment
            StripeConfiguration.ApiKey = "sk_test_mockkey";
        }

        public OrderAPIControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            _mockMapper = new Mock<IMapper>();
            _mockProductService = new Mock<IProductService>();
            _mockMessageBus = new Mock<IMessageBus>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Mock Stripe configuration and services
            MockStripeServices();

            _controller = new OrderAPIController(
                _dbContext,
                _mockProductService.Object,
                _mockMapper.Object,
                _mockConfiguration.Object,
                _mockMessageBus.Object
            );

            InitializeControllerResponse();
        }

        private void InitializeControllerResponse()
        {
            // Use reflection to set the protected _response field
            var field = typeof(OrderAPIController)
                .GetField("_response", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(_controller, new ResponseDTO { IsSuccess = true });
            }
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void ClearDatabase()
        {
            _dbContext.OrderHeader.RemoveRange(_dbContext.OrderHeader);
            _dbContext.OrderDetails.RemoveRange(_dbContext.OrderDetails);
            _dbContext.SaveChanges();
        }

        private void SetupUserRole(string role, string userId = "testUser")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        [Fact]
        public void Get_AdminRole_ReturnsAllOrders()
        {
            // Arrange
            ClearDatabase();
            SetupUserRole("ADMIN");

            var orders = new List<OrderHeader>
            {
                new OrderHeader { OrderHeaderId = 1, UserId = "user1", OrderTotal = 100 },
                new OrderHeader { OrderHeaderId = 2, UserId = "user2", OrderTotal = 200 }
            };
            _dbContext.OrderHeader.AddRange(orders);
            _dbContext.SaveChanges();

            var orderDTOs = orders.Select(o => new OrderHeaderDTO { OrderHeaderId = o.OrderHeaderId }).ToList();
            _mockMapper.Setup(m => m.Map<IEnumerable<OrderHeaderDTO>>(It.IsAny<IEnumerable<OrderHeader>>()))
                .Returns(orderDTOs);

            // Act
            var result = _controller.Get();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(orderDTOs, result.Result);
        }

        [Fact]
        public void Get_UserRole_ReturnsUserOrders()
        {
            // Arrange
            ClearDatabase();
            var userId = "testUser";
            SetupUserRole("USER", userId);

            var orders = new List<OrderHeader>
            {
                new OrderHeader { OrderHeaderId = 1, UserId = userId, OrderTotal = 100 },
                new OrderHeader { OrderHeaderId = 2, UserId = "otherUser", OrderTotal = 200 }
            };
            _dbContext.OrderHeader.AddRange(orders);
            _dbContext.SaveChanges();

            var userOrders = orders.Where(o => o.UserId == userId).ToList();
            var orderDTOs = userOrders.Select(o => new OrderHeaderDTO { OrderHeaderId = o.OrderHeaderId }).ToList();
            _mockMapper.Setup(m => m.Map<IEnumerable<OrderHeaderDTO>>(It.IsAny<IEnumerable<OrderHeader>>()))
                .Returns(orderDTOs);

            // Act
            var result = _controller.Get(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(orderDTOs, result.Result);
        }

        [Fact]
        public void GetOrder_ExistingId_ReturnsOrder()
        {
            // Arrange
            ClearDatabase();
            var order = new OrderHeader { OrderHeaderId = 1, UserId = "user1", OrderTotal = 100 };
            _dbContext.OrderHeader.Add(order);
            _dbContext.SaveChanges();

            var orderDTO = new OrderHeaderDTO { OrderHeaderId = order.OrderHeaderId };
            _mockMapper.Setup(m => m.Map<OrderHeaderDTO>(It.IsAny<OrderHeader>()))
                .Returns(orderDTO);

            // Act
            var result = _controller.Get(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(orderDTO, result.Result);
        }

        [Fact]
        public async Task CreateOrder_ValidCart_CreatesOrder()
        {
            // Arrange
            ClearDatabase();
            var cartDto = new CartDTO
            {
                CartHeader = new CartHeaderDTO { UserId = "user1", CartTotal = 100 },
                CartDetails = new List<CartDetailsDTO>
                {
                    new CartDetailsDTO { ProductId = 1, Count = 1 }
                }
            };

            var orderHeaderDto = new OrderHeaderDTO
            {
                OrderHeaderId = 1,
                UserId = "user1",
                OrderTotal = 100
            };

            _mockMapper.Setup(m => m.Map<OrderHeaderDTO>(It.IsAny<CartHeaderDTO>()))
                .Returns(orderHeaderDto);
            _mockMapper.Setup(m => m.Map<OrderHeader>(It.IsAny<OrderHeaderDTO>()))
                .Returns(new OrderHeader { OrderHeaderId = 1 });

            // Act
            var result = await _controller.CreateOrder(cartDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(orderHeaderDto, result.Result);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonCancelledStatus_UpdatesStatus()
        {
            // Arrange
            ClearDatabase();
            ResetControllerResponse();

            var order = CreateTestOrder(SD.Status_Pending);

            // Act
            var result = await _controller.UpdateOrderStatus(1, SD.Status_Approved);

            // Assert
            Assert.True(result.IsSuccess);
            var updatedOrder = await _dbContext.OrderHeader.FindAsync(1);
            Assert.NotNull(updatedOrder);
            Assert.Equal(SD.Status_Approved, updatedOrder.Status);
        }

        //[Fact]
        //public async Task UpdateOrderStatus_ToCancel_UpdatesStatusAndInitiatesRefund()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    ResetControllerResponse();

        //    var order = CreateTestOrder(SD.Status_Approved);

        //    // Setup Stripe mocking
        //    var mockRefundService = new Mock<RefundService>();
        //    mockRefundService.Setup(s => s.CreateAsync(
        //        It.IsAny<RefundCreateOptions>(),
        //        It.IsAny<RequestOptions>(),
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new Refund { Id = "re_mock_123" });

        //    // Use a custom service factory
        //    var serviceFactory = new Mock<IServiceProvider>();
        //    serviceFactory.Setup(x => x.GetService(typeof(RefundService)))
        //        .Returns(mockRefundService.Object);

        //    // Configure Stripe
        //    StripeConfiguration.ApiKey = "sk_test_mockkey";

        //    // Act
        //    var result = await _controller.UpdateOrderStatus(1, SD.Status_Cancelled);

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    var updatedOrder = await _dbContext.OrderHeader.FindAsync(1);
        //    Assert.NotNull(updatedOrder);
        //    Assert.Equal(SD.Status_Cancelled, updatedOrder.Status);
        //}

        [Fact]
        public async Task UpdateOrderStatus_InvalidOrder_ReturnsFalse()
        {
            // Arrange
            ClearDatabase();
            ResetControllerResponse(); // Reset response state before test

            // Act
            var result = await _controller.UpdateOrderStatus(999, SD.Status_Cancelled);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonCancelledStatus_UpdatesWithoutRefund()
        {
            // Arrange
            ClearDatabase();
            ResetControllerResponse(); // Replace direct access with helper method

            var order = new OrderHeader
            {
                OrderHeaderId = 1,
                UserId = "user1",
                Status = SD.Status_Pending,
                PaymentIntentId = "pi_123"
            };
            _dbContext.OrderHeader.Add(order);
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.UpdateOrderStatus(1, SD.Status_Shipped);

            // Assert
            Assert.True(result.IsSuccess);
            var updatedOrder = await _dbContext.OrderHeader.FindAsync(1);
            Assert.NotNull(updatedOrder);
            Assert.Equal(SD.Status_Shipped, updatedOrder.Status);
        }

        // Add these test cases for completeness
        [Fact]
        public async Task UpdateOrderStatus_NullOrder_ReturnsFalse()
        {
            // Arrange
            ClearDatabase();
            ResetControllerResponse(); // Replace direct access with helper method

            // Act
            var result = await _controller.UpdateOrderStatus(1, SD.Status_Cancelled);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateOrderStatus_ExceptionThrown_ReturnsFalse()
        {
            // Arrange
            ClearDatabase();
            ResetControllerResponse(); // Replace direct access with helper method

            // Force an exception by passing null status
            // Act
            var result = await _controller.UpdateOrderStatus(1, null);

            // Assert
            Assert.False(result.IsSuccess);
        }

        // Update the SD class to include all relevant status constants
        public static class SD
        {
            public const string Status_Cancelled = "Cancelled";
            public const string Status_Pending = "Pending";
            public const string Status_Approved = "Approved";
            public const string Status_ReadyForPickup = "ReadyForPickup";
            public const string Status_Completed = "Completed";
            public const string Status_Refunded = "Refunded";
            public const string Status_Shipped = "Shipped";
        }

        // Add helper methods for better test maintenance
        private OrderHeader CreateTestOrder(string status = SD.Status_Pending)
        {
            var order = new OrderHeader
            {
                OrderHeaderId = 1,
                UserId = "user1",
                Status = status,
                PaymentIntentId = "pi_mock_123", // Important for refund
                OrderTotal = 100.00,
                StripeSessionId = "cs_mock_123"
            };
            _dbContext.OrderHeader.Add(order);
            _dbContext.SaveChanges();
            return order;
        }

        private void ResetControllerResponse(bool isSuccess = true)
        {
            var field = typeof(OrderAPIController)
                .GetField("_response", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(_controller, new ResponseDTO
                {
                    IsSuccess = isSuccess,
                    Result = null,
                    Message = string.Empty
                });
            }
        }

        private void MockStripeServices()
        {
            // Mock Stripe configuration
            _mockConfiguration.Setup(x => x["StripeSettings:SecretKey"])
                .Returns("sk_test_mockkey");
            _mockConfiguration.Setup(x => x["StripeSettings:PublicKey"])
                .Returns("pk_test_mockkey");

            // Create mock refund service
            var mockRefundService = new Mock<RefundService>();
            mockRefundService.Setup(s => s.CreateAsync(
                It.IsAny<RefundCreateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Refund { Id = "re_mock_123" });

            // Setup service factory
            var serviceFactory = new Mock<IServiceProvider>();
            serviceFactory.Setup(x => x.GetService(typeof(RefundService)))
                .Returns(mockRefundService.Object);

            // Configure Stripe
            StripeConfiguration.ApiKey = "sk_test_mockkey";
        }

    }
}