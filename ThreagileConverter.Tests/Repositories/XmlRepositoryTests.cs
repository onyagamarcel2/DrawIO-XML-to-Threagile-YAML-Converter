using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Repositories;

namespace ThreagileConverter.Tests.Repositories
{
    public class XmlRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly Mock<ILogger<XmlRepository>> _loggerMock;
        private readonly XmlRepository _repository;

        public XmlRepositoryTests()
        {
            _testFilePath = Path.GetTempFileName();
            _loggerMock = new Mock<ILogger<XmlRepository>>();
            _repository = new XmlRepository(_testFilePath, _loggerMock.Object);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WhenElementExists_ReturnsElement()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));
            await _repository.AddAsync(element);

            // Act
            var result = await _repository.GetByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Attribute("id")?.Value);
        }

        [Fact]
        public async Task GetByIdAsync_WhenElementDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllElements()
        {
            // Arrange
            var element1 = new XElement("test", new XAttribute("id", "1"));
            var element2 = new XElement("test", new XAttribute("id", "2"));
            await _repository.AddAsync(element1);
            await _repository.AddAsync(element2);

            // Act
            var results = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task AddAsync_AddsElementToDocument()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));

            // Act
            var result = await _repository.AddAsync(element);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Attribute("id")?.Value);
            var allElements = await _repository.GetAllAsync();
            Assert.Single(allElements);
        }

        [Fact]
        public async Task UpdateAsync_WhenElementExists_UpdatesElement()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));
            await _repository.AddAsync(element);
            var updatedElement = new XElement("test", new XAttribute("id", "1"), "updated");

            // Act
            var result = await _repository.UpdateAsync(updatedElement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("updated", result.Value);
        }

        [Fact]
        public async Task UpdateAsync_WhenElementDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(element));
        }

        [Fact]
        public async Task DeleteAsync_WhenElementExists_ReturnsTrue()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));
            await _repository.AddAsync(element);

            // Act
            var result = await _repository.DeleteAsync("1");

            // Assert
            Assert.True(result);
            var allElements = await _repository.GetAllAsync();
            Assert.Empty(allElements);
        }

        [Fact]
        public async Task DeleteAsync_WhenElementDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _repository.DeleteAsync("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenElementExists_ReturnsTrue()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));
            await _repository.AddAsync(element);

            // Act
            var result = await _repository.ExistsAsync("1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenElementDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var element1 = new XElement("test", new XAttribute("id", "1"));
            var element2 = new XElement("test", new XAttribute("id", "2"));
            await _repository.AddAsync(element1);
            await _repository.AddAsync(element2);

            // Act
            var count = await _repository.CountAsync();

            // Assert
            Assert.Equal(2, count);
        }
    }
} 