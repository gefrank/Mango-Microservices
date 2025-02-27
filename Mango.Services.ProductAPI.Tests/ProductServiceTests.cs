using AutoMapper;
using Mango.Services.ProductAPI.Controllers;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Mango.Services.ProductAPI.Tests
{
    public class ProductAPIControllerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductAPIController _controller;

        public ProductAPIControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique database name for each test
                .Options;
            _dbContext = new AppDbContext(options);

            _mockMapper = new Mock<IMapper>();

            _controller = new ProductAPIController(_dbContext, _mockMapper.Object);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void ClearDatabase()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Get_ReturnsAllProducts()
        {
            ClearDatabase();
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product1", Price = 10, CategoryName = "Category1", Description = "Description1" },
                new Product { ProductId = 2, Name = "Product2", Price = 20, CategoryName = "Category2", Description = "Description2" }
            };
            _dbContext.Products.AddRange(products);
            _dbContext.SaveChanges();

            var productDTOs = products.Select(p => new ProductDTO { ProductId = p.ProductId, Name = p.Name, Price = p.Price, CategoryName = p.CategoryName, Description = p.Description }).ToList();
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(It.IsAny<IEnumerable<Product>>())).Returns(productDTOs);

            // Act
            var result = _controller.Get();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productDTOs, result.Result);
        }

        [Fact]
        public void Get_WithId_ReturnsProduct()
        {
            ClearDatabase();
            // Arrange
            var product = new Product { ProductId = 1, Name = "Product1", Price = 10, CategoryName = "Category1", Description = "Description1" };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var productDTO = new ProductDTO { ProductId = product.ProductId, Name = product.Name, Price = product.Price, CategoryName = product.CategoryName, Description = product.Description };
            _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            // Act
            var result = _controller.Get(product.ProductId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productDTO, result.Result);
        }

        [Fact]
        public void Post_AddsProduct()
        {
            ClearDatabase();
            // Arrange
            var productDTO = new ProductDTO { Name = "Product1", Price = 10, CategoryName = "Category1", Description = "Description1" };
            var product = new Product { ProductId = 1, Name = productDTO.Name, Price = productDTO.Price, CategoryName = productDTO.CategoryName, Description = productDTO.Description };
            _mockMapper.Setup(m => m.Map<Product>(It.IsAny<ProductDTO>())).Returns(product);
            _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            // Act
            var result = _controller.Post(productDTO);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productDTO, result.Result);
        }

        [Fact]
        public void Put_UpdatesProduct()
        {
            ClearDatabase();
            // Arrange
            var product = new Product { ProductId = 1, Name = "Product1", Price = 10, CategoryName = "Category1", Description = "Description1" };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var productDTO = new ProductDTO { ProductId = product.ProductId, Name = "UpdatedProduct", Price = 20, CategoryName = "UpdatedCategory", Description = "UpdatedDescription" };
            _mockMapper.Setup(m => m.Map<Product>(It.IsAny<ProductDTO>())).Returns(product);
            _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>())).Returns(productDTO);

            // Act
            var result = _controller.Put(productDTO);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productDTO, result.Result);
        }

        [Fact]
        public void Delete_RemovesProduct()
        {
            ClearDatabase();
            // Arrange
            var product = new Product { ProductId = 1, Name = "Product1", Price = 10, CategoryName = "Category1", Description = "Description1" };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            // Act
            var result = _controller.Delete(product.ProductId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(_dbContext.Products.Find(product.ProductId));
        }
    }
}