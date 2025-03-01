using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Controllers;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Mango.Services.ShoppingCartAPI.Tests
{
    public class ShoppingCartAPIControllerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICouponService> _mockCouponService;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly CartAPIController _controller;

        public ShoppingCartAPIControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            _mockMapper = new Mock<IMapper>();
            _mockProductService = new Mock<IProductService>();
            _mockCouponService = new Mock<ICouponService>();
            _mockMessageBus = new Mock<IMessageBus>();
            _mockConfiguration = new Mock<IConfiguration>();

            _controller = new CartAPIController(
                _dbContext,
                _mockMapper.Object,
                _mockProductService.Object,
                _mockCouponService.Object,
                _mockMessageBus.Object,
                _mockConfiguration.Object
            );
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void ClearDatabase()
        {
            _dbContext.CartHeaders.RemoveRange(_dbContext.CartHeaders);
            _dbContext.CartDetails.RemoveRange(_dbContext.CartDetails);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetCart_ReturnsCartForUser()
        {
            // Arrange
            ClearDatabase();
            var userId = "testUser";
            var cartHeader = new CartHeader
            {
                CartHeaderId = 1,
                UserId = userId,
                CouponCode = ""
            };
            var cartDetails = new CartDetails
            {
                CartDetailsId = 1,
                CartHeaderId = 1,
                ProductId = 1,
                Count = 1
            };

            _dbContext.CartHeaders.Add(cartHeader);
            _dbContext.CartDetails.Add(cartDetails);
            _dbContext.SaveChanges();

            var cartHeaderDto = new CartHeaderDTO { CartHeaderId = 1, UserId = userId };
            var cartDetailsDto = new CartDetailsDTO { CartDetailsId = 1, Count = 1, ProductId = 1 };
            var products = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "Test Product", Price = 10 }
            };

            _mockMapper.Setup(m => m.Map<CartHeaderDTO>(It.IsAny<CartHeader>())).Returns(cartHeaderDto);
            _mockMapper.Setup(m => m.Map<IEnumerable<CartDetailsDTO>>(It.IsAny<IEnumerable<CartDetails>>()))
                .Returns(new List<CartDetailsDTO> { cartDetailsDto });
            _mockProductService.Setup(p => p.GetProducts()).ReturnsAsync(products);

            // Act
            var response = await _controller.GetCart(userId);

            // Assert
            Assert.True(response.IsSuccess);
            var cart = response.Result as CartDTO;
            Assert.NotNull(cart);
            Assert.Equal(userId, cart.CartHeader.UserId);
            Assert.Single(cart.CartDetails);
        }

        [Fact]
        public async Task CartUpsert_NewCart_CreatesCartSuccessfully()
        {
            // Arrange
            ClearDatabase();
            var cartDto = new CartDTO
            {
                CartHeader = new CartHeaderDTO { UserId = "testUser" },
                CartDetails = new List<CartDetailsDTO>
                {
                    new CartDetailsDTO { ProductId = 1, Count = 1 }
                }
            };

            var cartHeader = new CartHeader { CartHeaderId = 1, UserId = "testUser" };
            var cartDetails = new CartDetails { CartDetailsId = 1, CartHeaderId = 1, ProductId = 1, Count = 1 };

            _mockMapper.Setup(m => m.Map<CartHeader>(It.IsAny<CartHeaderDTO>())).Returns(cartHeader);
            _mockMapper.Setup(m => m.Map<CartDetails>(It.IsAny<CartDetailsDTO>())).Returns(cartDetails);

            // Act
            var response = await _controller.CartUpsert(cartDto);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Result);
            var savedCart = await _dbContext.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == "testUser");
            Assert.NotNull(savedCart);
        }

        [Fact]
        public async Task RemoveCart_ExistingCart_RemovesSuccessfully()
        {
            // Arrange
            ClearDatabase();
            var cartHeader = new CartHeader { CartHeaderId = 1, UserId = "testUser" };
            var cartDetails = new CartDetails { CartDetailsId = 1, CartHeaderId = 1, ProductId = 1, Count = 1 };

            _dbContext.CartHeaders.Add(cartHeader);
            _dbContext.CartDetails.Add(cartDetails);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _controller.RemoveCart(1);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Null(await _dbContext.CartDetails.FindAsync(1));
            Assert.Null(await _dbContext.CartHeaders.FindAsync(1));
        }

        [Fact]
        public async Task ApplyCoupon_ValidCoupon_AppliesSuccessfully()
        {
            // Arrange
            ClearDatabase();
            var cartHeader = new CartHeader { CartHeaderId = 1, UserId = "testUser" };
            _dbContext.CartHeaders.Add(cartHeader);
            await _dbContext.SaveChangesAsync();

            var cartDto = new CartDTO
            {
                CartHeader = new CartHeaderDTO { UserId = "testUser", CouponCode = "TEST10" }
            };

            // Act
            var response = await _controller.ApplyCoupon(cartDto);

            // Assert
            Assert.True(((ResponseDTO)response).IsSuccess); // Cast the object response to ResponseDTO
            var updatedCart = await _dbContext.CartHeaders.FirstAsync(ch => ch.UserId == "testUser");
            Assert.Equal("TEST10", updatedCart.CouponCode);
        }

        [Fact]
        public async Task GetCart_WithCoupon_AppliesDiscountCorrectly()
        {
            // Arrange
            ClearDatabase();
            var userId = "testUser";
            var cartHeader = new CartHeader
            {
                CartHeaderId = 1,
                UserId = userId,
                CouponCode = "TEST10"
            };
            var cartDetails = new CartDetails
            {
                CartDetailsId = 1,
                CartHeaderId = 1,
                ProductId = 1,
                Count = 1
            };

            _dbContext.CartHeaders.Add(cartHeader);
            _dbContext.CartDetails.Add(cartDetails);
            await _dbContext.SaveChangesAsync();

            var cartHeaderDto = new CartHeaderDTO { CartHeaderId = 1, UserId = userId, CouponCode = "TEST10" };
            var cartDetailsDto = new CartDetailsDTO { CartDetailsId = 1, Count = 1, ProductId = 1 };
            var products = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "Test Product", Price = 100 }
            };
            var couponDto = new CouponDTO { CouponCode = "TEST10", DiscountAmount = 10, MinAmount = 50 };

            _mockMapper.Setup(m => m.Map<CartHeaderDTO>(It.IsAny<CartHeader>())).Returns(cartHeaderDto);
            _mockMapper.Setup(m => m.Map<IEnumerable<CartDetailsDTO>>(It.IsAny<IEnumerable<CartDetails>>()))
                .Returns(new List<CartDetailsDTO> { cartDetailsDto });
            _mockProductService.Setup(p => p.GetProducts()).ReturnsAsync(products);
            _mockCouponService.Setup(c => c.GetCoupon(It.IsAny<string>())).ReturnsAsync(couponDto);

            // Act
            var response = await _controller.GetCart(userId);

            // Assert
            Assert.True(response.IsSuccess);
            var cart = response.Result as CartDTO;
            Assert.NotNull(cart);
            Assert.Equal(90, cart.CartHeader.CartTotal); // 100 - 10 discount
            Assert.Equal(10, cart.CartHeader.Discount);
        }
    }
}