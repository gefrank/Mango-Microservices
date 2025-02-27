using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Services.AuthAPI.Tests
{
    public class AuthServiceTests
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly Mock<DbSet<ApplicationUser>> _mockDbSet;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique name for each test
                .Options;
            _dbContext = new AppDbContext(options);

            _mockUserManager = MockUserManager<ApplicationUser>();
            _mockRoleManager = MockRoleManager<IdentityRole>();
            _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _mockDbSet = new Mock<DbSet<ApplicationUser>>();

            _authService = new AuthService(
                _dbContext,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockJwtTokenGenerator.Object
            );
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO { UserName = "test@test.com", Password = "Test123!" };
            var user = new ApplicationUser
            {
                UserName = loginRequest.UserName,
                Email = "test@test.com",
                Name = "Test User",
                Id = "test-user-id" // Add an Id as it might be needed
            };
            var roles = new List<string> { "User" };

            // Set up the mock DbSet with the test user
            var users = new List<ApplicationUser> { user }.AsQueryable();
            _mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            _mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            _mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            _mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Add the user to the in-memory database
            await _dbContext.ApplicationUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Set up UserManager mocks
            _mockUserManager.Setup(um => um.FindByNameAsync(loginRequest.UserName))
                .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), loginRequest.Password))
                .ReturnsAsync(true);
            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(roles);

            // Set up JWT generator mock
            _mockJwtTokenGenerator.Setup(jwt => jwt.GenerateToken(It.IsAny<ApplicationUser>(), roles))
                .Returns("test-token");

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal("test-token", result.Token);
            Assert.Equal(user.Email, result.User.Email);
            Assert.Equal(user.Name, result.User.Name);
        }

        [Fact]
        public async Task AssignRole_UserNotFound_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@test.com";
            string roleName = "Admin";
            var users = new List<ApplicationUser>().AsQueryable();

            SetupMockDbSet(_mockDbSet, users);

            // Act
            var result = await _authService.AssignRole(email, roleName);

            // Assert
            Assert.False(result);
        }

        private static void SetupMockDbSet<T>(Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            options.Setup(o => o.Value).Returns(idOptions);
            var idValidator = new Mock<IUserValidator<TUser>>();
            var pwdValidator = new Mock<IPasswordValidator<TUser>>();
            var pwdHasher = new Mock<IPasswordHasher<TUser>>();
            var lockoutManager = new Mock<ILookupNormalizer>();
            var error = new Mock<IdentityErrorDescriber>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<TUser>>>();

            return new Mock<UserManager<TUser>>(
                store.Object,
                options.Object,
                pwdHasher.Object,
                new IUserValidator<TUser>[] { idValidator.Object },
                new IPasswordValidator<TUser>[] { pwdValidator.Object },
                lockoutManager.Object,
                error.Object,
                serviceProvider.Object,
                logger.Object);
        }

        private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
        {
            var store = new Mock<IRoleStore<TRole>>();
            return new Mock<RoleManager<TRole>>(store.Object, null, null, null, null);
        }
    }
}