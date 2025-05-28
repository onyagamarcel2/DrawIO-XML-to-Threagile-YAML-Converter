using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Repositories;

namespace ThreagileConverter.IntegrationTests.Repositories
{
    public class XmlRepositoryIntegrationTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly ILogger<XmlRepository> _logger;
        private readonly XmlRepository _repository;

        public XmlRepositoryIntegrationTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xml");
            _logger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<XmlRepository>();
            _repository = new XmlRepository(_testFilePath, _logger);

            // Créer un fichier XML initial valide
            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("root")
            );
            doc.Save(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public async Task FullCrudCycle_ShouldWorkCorrectly()
        {
            // Arrange
            var element1 = new XElement("test", new XAttribute("id", "1"), "value1");
            var element2 = new XElement("test", new XAttribute("id", "2"), "value2");

            // Act & Assert - Create
            var added1 = await _repository.AddAsync(element1);
            var added2 = await _repository.AddAsync(element2);
            Assert.NotNull(added1);
            Assert.NotNull(added2);
            Assert.Equal(2, await _repository.CountAsync());

            // Act & Assert - Read
            var retrieved1 = await _repository.GetByIdAsync("1");
            var retrieved2 = await _repository.GetByIdAsync("2");
            Assert.NotNull(retrieved1);
            Assert.NotNull(retrieved2);
            Assert.Equal("value1", retrieved1.Value);
            Assert.Equal("value2", retrieved2.Value);

            // Act & Assert - Update
            var updatedElement = new XElement("test", new XAttribute("id", "1"), "updated");
            var updated = await _repository.UpdateAsync(updatedElement);
            Assert.NotNull(updated);
            Assert.Equal("updated", updated.Value);

            // Act & Assert - Delete
            var deleted = await _repository.DeleteAsync("2");
            Assert.True(deleted);
            Assert.Equal(1, await _repository.CountAsync());
            Assert.Null(await _repository.GetByIdAsync("2"));
        }

        [Fact]
        public async Task ConcurrentOperations_ShouldMaintainConsistency()
        {
            // Arrange
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                var element = new XElement("test", new XAttribute("id", i.ToString()), $"value{i}");
                tasks[i] = _repository.AddAsync(element);
            }

            // Act
            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, await _repository.CountAsync());
            for (int i = 0; i < 10; i++)
            {
                var element = await _repository.GetByIdAsync(i.ToString());
                Assert.NotNull(element);
                Assert.Equal($"value{i}", element.Value);
            }
        }

        [Fact]
        public async Task LargeDocument_ShouldHandleCorrectly()
        {
            // Arrange
            const int elementCount = 1000;
            var tasks = new Task[elementCount];

            // Act
            for (int i = 0; i < elementCount; i++)
            {
                var element = new XElement("test", 
                    new XAttribute("id", i.ToString()),
                    new XElement("data", new string('x', 1000))
                );
                tasks[i] = _repository.AddAsync(element);
            }
            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(elementCount, await _repository.CountAsync());
            var fileInfo = new FileInfo(_testFilePath);
            Assert.True(fileInfo.Length > 0);
        }

        [Fact]
        public async Task InvalidOperations_ShouldHandleGracefully()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"));

            // Act & Assert - Try to update non-existent element
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _repository.UpdateAsync(element));

            // Act & Assert - Try to delete non-existent element
            var deleted = await _repository.DeleteAsync("nonexistent");
            Assert.False(deleted);

            // Act & Assert - Try to get non-existent element
            var retrieved = await _repository.GetByIdAsync("nonexistent");
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task FileSystemOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "1"), "value1");
            await _repository.AddAsync(element);

            // Act & Assert - File exists
            Assert.True(File.Exists(_testFilePath));

            // Act & Assert - File is readable
            var content = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains("value1", content);

            // Act & Assert - File is writable
            var updatedElement = new XElement("test", new XAttribute("id", "1"), "updated");
            await _repository.UpdateAsync(updatedElement);
            content = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains("updated", content);
        }
    }
} 