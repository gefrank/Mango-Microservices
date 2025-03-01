using AutoMapper;
using Mango.Services.CouponAPI.Controllers;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;
using System.Security.Claims;

namespace Mango.Services.CouponAPI.Tests
{
    public class CouponAPIControllerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CouponAPIController _controller;

        public CouponAPIControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            _mockMapper = new Mock<IMapper>();

            _controller = new CouponAPIController(_dbContext, _mockMapper.Object);

            // Initialize ResponseDTO
            var field = typeof(CouponAPIController)
                .GetField("_response", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(_controller, new ResponseDTO
                {
                    IsSuccess = true,
                    Message = string.Empty
                });
            }

            // Setup default authorization context
            SetupControllerContext();
        }

        private void SetupControllerContext(string role = "ADMIN")
        {
            // Create identity with authentication
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "testUser"),
        new Claim(ClaimTypes.Role, role),
        new Claim(ClaimTypes.NameIdentifier, "testUser")
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            // Create mock authorization service
            var authServiceMock = new Mock<IAuthorizationService>();
            authServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync(role == "ADMIN"
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());

            // Add authorization service to HttpContext
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IAuthorizationService)))
                .Returns(authServiceMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void ClearDatabase()
        {
            _dbContext.Coupons.RemoveRange(_dbContext.Coupons);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Get_ReturnsAllCoupons()
        {
            // Arrange
            ClearDatabase();
            var coupons = new List<Coupon>
            {
                new Coupon { CouponId = 1, CouponCode = "10OFF", DiscountAmount = 10, MinAmount = 20 },
                new Coupon { CouponId = 2, CouponCode = "20OFF", DiscountAmount = 20, MinAmount = 40 }
            };
            _dbContext.Coupons.AddRange(coupons);
            _dbContext.SaveChanges();

            var couponDTOs = coupons.Select(c => new CouponDTO
            {
                CouponId = c.CouponId,
                CouponCode = c.CouponCode,
                DiscountAmount = c.DiscountAmount,
                MinAmount = c.MinAmount
            }).ToList();

            _mockMapper.Setup(m => m.Map<IEnumerable<CouponDTO>>(It.IsAny<IEnumerable<Coupon>>()))
                .Returns(couponDTOs);

            // Act
            var result = _controller.Get();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(couponDTOs, result.Result);
        }

        [Fact]
        public void Get_WithId_ReturnsCoupon()
        {
            // Arrange
            ClearDatabase();
            var coupon = new Coupon
            {
                CouponId = 1,
                CouponCode = "10OFF",
                DiscountAmount = 10,
                MinAmount = 20
            };
            _dbContext.Coupons.Add(coupon);
            _dbContext.SaveChanges();

            var couponDTO = new CouponDTO
            {
                CouponId = coupon.CouponId,
                CouponCode = coupon.CouponCode,
                DiscountAmount = coupon.DiscountAmount,
                MinAmount = coupon.MinAmount
            };

            _mockMapper.Setup(m => m.Map<CouponDTO>(It.IsAny<Coupon>()))
                .Returns(couponDTO);

            // Act
            var result = _controller.Get(coupon.CouponId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(couponDTO, result.Result);
        }

        [Fact]
        public void GetByCode_ReturnsCoupon()
        {
            // Arrange
            ClearDatabase();
            var coupon = new Coupon
            {
                CouponId = 1,
                CouponCode = "10OFF",
                DiscountAmount = 10,
                MinAmount = 20
            };
            _dbContext.Coupons.Add(coupon);
            _dbContext.SaveChanges();

            var couponDTO = new CouponDTO
            {
                CouponId = coupon.CouponId,
                CouponCode = coupon.CouponCode,
                DiscountAmount = coupon.DiscountAmount,
                MinAmount = coupon.MinAmount
            };

            _mockMapper.Setup(m => m.Map<CouponDTO>(It.IsAny<Coupon>()))
                .Returns(couponDTO);

            // Act
            var result = _controller.GetByCode(coupon.CouponCode);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(couponDTO, result.Result);
        }

        //[Fact]
        //public void Post_AddsCoupon()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    SetupControllerContext("ADMIN"); // Ensure ADMIN role
        //    var couponDTO = new CouponDTO
        //    {
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };

        //    var coupon = new Coupon
        //    {
        //        CouponCode = couponDTO.CouponCode,
        //        DiscountAmount = couponDTO.DiscountAmount,
        //        MinAmount = couponDTO.MinAmount
        //    };

        //    // Setup mapper to handle both the initial mapping and the return mapping
        //    _mockMapper.Setup(m => m.Map<Coupon>(It.Is<CouponDTO>(dto =>
        //        dto.CouponCode == couponDTO.CouponCode &&
        //        dto.DiscountAmount == couponDTO.DiscountAmount &&
        //        dto.MinAmount == couponDTO.MinAmount)))
        //        .Returns(coupon);

        //    _mockMapper.Setup(m => m.Map<CouponDTO>(It.Is<Coupon>(c =>
        //        c.CouponCode == coupon.CouponCode &&
        //        c.DiscountAmount == coupon.DiscountAmount &&
        //        c.MinAmount == coupon.MinAmount)))
        //        .Returns(couponDTO);

        //    // Act
        //    var result = _controller.Post(couponDTO);

        //    // Assert
        //    Assert.True(result.IsSuccess, $"Expected IsSuccess to be True but got False. Message: {result.Message}");
        //    Assert.Equal(couponDTO, result.Result);

        //    var savedCoupon = _dbContext.Coupons.FirstOrDefault(c => c.CouponCode == "10OFF");
        //    Assert.NotNull(savedCoupon);
        //    Assert.Equal(couponDTO.CouponCode, savedCoupon.CouponCode);
        //    Assert.Equal(couponDTO.DiscountAmount, savedCoupon.DiscountAmount);
        //    Assert.Equal(couponDTO.MinAmount, savedCoupon.MinAmount);
        //}

        //[Fact]
        //public void Post_NonAdminRole_ReturnsFalse()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    SetupControllerContext("USER"); // Non-admin role
        //    var couponDTO = new CouponDTO
        //    {
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };

        //    try
        //    {
        //        // Act
        //        var result = _controller.Post(couponDTO);

        //        // Assert
        //        Assert.False(result.IsSuccess);
        //        Assert.Contains("Unauthorized", result.Message);
        //    }
        //    catch (Exception ex) when (ex is UnauthorizedAccessException)
        //    {
        //        // If authorization throws an exception, that's also a valid test case
        //        Assert.True(true);
        //    }
        //    finally
        //    {
        //        // Verify no coupon was added
        //        var savedCoupon = _dbContext.Coupons.FirstOrDefault(c => c.CouponCode == "10OFF");
        //        Assert.Null(savedCoupon);
        //    }
        //}

        //[Fact]
        //public void Put_UpdatesCoupon()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    SetupControllerContext("ADMIN"); // Ensure ADMIN role
        //    var coupon = new Coupon
        //    {
        //        CouponId = 1,
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };
        //    _dbContext.Coupons.Add(coupon);
        //    _dbContext.SaveChanges();

        //    var couponDTO = new CouponDTO
        //    {
        //        CouponId = coupon.CouponId,
        //        CouponCode = "20OFF",
        //        DiscountAmount = 20,
        //        MinAmount = 40
        //    };

        //    _mockMapper.Setup(m => m.Map<Coupon>(It.IsAny<CouponDTO>()))
        //        .Returns(coupon);
        //    _mockMapper.Setup(m => m.Map<CouponDTO>(It.IsAny<Coupon>()))
        //        .Returns(couponDTO);

        //    // Act
        //    var result = _controller.Put(couponDTO);

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.Equal(couponDTO, result.Result);
        //    var updatedCoupon = _dbContext.Coupons.Find(1);
        //    Assert.Equal("20OFF", updatedCoupon.CouponCode);
        //}

        //[Fact]
        //public void Put_NonAdminRole_ReturnsFalse()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    var originalCoupon = new Coupon
        //    {
        //        CouponId = 1,
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };
        //    _dbContext.Coupons.Add(originalCoupon);
        //    _dbContext.SaveChanges();

        //    SetupControllerContext("USER"); // Non-admin role

        //    var couponDTO = new CouponDTO
        //    {
        //        CouponId = originalCoupon.CouponId,
        //        CouponCode = "20OFF",
        //        DiscountAmount = 20,
        //        MinAmount = 40
        //    };

        //    try
        //    {
        //        // Act
        //        var result = _controller.Put(couponDTO);

        //        // Assert
        //        Assert.False(result.IsSuccess);
        //        Assert.Contains("Unauthorized", result.Message);
        //    }
        //    catch (Exception ex) when (ex is UnauthorizedAccessException)
        //    {
        //        // If authorization throws an exception, that's also a valid test case
        //        Assert.True(true);
        //    }
        //    finally
        //    {
        //        // Verify coupon wasn't modified
        //        var existingCoupon = _dbContext.Coupons.Find(originalCoupon.CouponId);
        //        Assert.NotNull(existingCoupon);
        //        Assert.Equal("10OFF", existingCoupon.CouponCode);
        //        Assert.Equal(10, existingCoupon.DiscountAmount);
        //        Assert.Equal(20, existingCoupon.MinAmount);
        //    }
        //}

        //[Fact]
        //public void Delete_RemovesCoupon()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    var coupon = new Coupon
        //    {
        //        CouponId = 1,
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };
        //    _dbContext.Coupons.Add(coupon);
        //    _dbContext.SaveChanges();

        //    // Ensure ADMIN role is set
        //    SetupControllerContext("ADMIN");

        //    // Act
        //    var result = _controller.Delete(coupon.CouponId);

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.Null(_dbContext.Coupons.Find(coupon.CouponId));
        //}

        //[Fact]
        //public async Task Delete_NonAdminRole_ReturnsFalse()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    var coupon = new Coupon
        //    {
        //        CouponId = 1,
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };
        //    await _dbContext.Coupons.AddAsync(coupon);
        //    await _dbContext.SaveChangesAsync();

        //    // Set non-admin role
        //    SetupControllerContext("USER");

        //    try
        //    {
        //        // Act
        //        var result = _controller.Delete(coupon.CouponId);

        //        // Assert
        //        Assert.False(result.IsSuccess);
        //        Assert.Contains("Unauthorized", result.Message);
        //    }
        //    catch (Exception ex) when (ex is UnauthorizedAccessException)
        //    {
        //        // If authorization throws an exception, that's also a valid test case
        //        Assert.True(true);
        //    }
        //    finally
        //    {
        //        // Verify coupon wasn't deleted
        //        var existingCoupon = await _dbContext.Coupons.FindAsync(coupon.CouponId);
        //        Assert.NotNull(existingCoupon);
        //        Assert.Equal("10OFF", existingCoupon.CouponCode);
        //        Assert.Equal(10, existingCoupon.DiscountAmount);
        //    }
        //}

        //[Fact]
        //public void GetByCode_InvalidCode_ReturnsNull()
        //{
        //    // Arrange
        //    ClearDatabase();

        //    // Act
        //    var result = _controller.GetByCode("INVALID");

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.Null(result.Result);
        //}

        //[Fact]
        //public void Post_DuplicateCouponCode_ReturnsFalse()
        //{
        //    // Arrange
        //    ClearDatabase();
        //    var existingCoupon = new Coupon
        //    {
        //        CouponId = 1,
        //        CouponCode = "10OFF",
        //        DiscountAmount = 10,
        //        MinAmount = 20
        //    };
        //    _dbContext.Coupons.Add(existingCoupon);
        //    _dbContext.SaveChanges();

        //    var couponDTO = new CouponDTO
        //    {
        //        CouponCode = "10OFF",
        //        DiscountAmount = 15,
        //        MinAmount = 30
        //    };

        //    // Act
        //    var result = _controller.Post(couponDTO);

        //    // Assert
        //    Assert.False(result.IsSuccess);
        //    Assert.Contains("Coupon code already exists", result.Message);
        //}


        [Fact]
        public void Get_NoAuthorizationRequired_ReturnsSuccess()
        {
            // Arrange
            ClearDatabase();
            SetupControllerContext("USER"); // Non-admin role is fine for Get
            var coupon = new Coupon
            {
                CouponId = 1,
                CouponCode = "10OFF",
                DiscountAmount = 10,
                MinAmount = 20
            };
            _dbContext.Coupons.Add(coupon);
            _dbContext.SaveChanges();

            var couponDTO = new CouponDTO
            {
                CouponId = coupon.CouponId,
                CouponCode = coupon.CouponCode,
                DiscountAmount = coupon.DiscountAmount,
                MinAmount = coupon.MinAmount
            };

            _mockMapper.Setup(m => m.Map<CouponDTO>(It.IsAny<Coupon>()))
                .Returns(couponDTO);

            // Act
            var result = _controller.Get(coupon.CouponId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(couponDTO, result.Result);
        }
    }
}