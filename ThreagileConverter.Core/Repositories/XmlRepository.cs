using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;
using System.Threading;

namespace ThreagileConverter.Core.Repositories
{
    /// <summary>
    /// Implémentation du repository pour les fichiers XML
    /// </summary>
    public class XmlRepository : IRepository<XElement>
    {
        private readonly string _filePath;
        private readonly ILogger<XmlRepository> _logger;
        private XDocument _document;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public XmlRepository(string filePath, ILogger<XmlRepository> logger)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LoadDocument();
        }

        private void LoadDocument()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        _document = XDocument.Load(_filePath);
                        if (_document.Root == null)
                        {
                            _document = new XDocument(new XElement("root"));
                            SaveDocument();
                        }
                        _logger.LogInformation("Document XML chargé depuis {FilePath}", _filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erreur lors du chargement du document XML, création d'un nouveau document");
                        _document = new XDocument(new XElement("root"));
                        SaveDocument();
                    }
                }
                else
                {
                    _document = new XDocument(new XElement("root"));
                    SaveDocument();
                    _logger.LogInformation("Nouveau document XML créé");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du document XML depuis {FilePath}", _filePath);
                throw;
            }
        }

        private async Task SaveDocumentAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                _document.Save(_filePath);
                _logger.LogInformation("Document XML sauvegardé dans {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du document XML dans {FilePath}", _filePath);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private void SaveDocument()
        {
            _fileLock.Wait();
            try
            {
                _document.Save(_filePath);
                _logger.LogInformation("Document XML sauvegardé dans {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du document XML dans {FilePath}", _filePath);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<XElement> GetByIdAsync(string id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var element = _document.Descendants()
                    .FirstOrDefault(e => e.Attribute("id")?.Value == id);
                
                if (element != null)
                {
                    _logger.LogDebug("Élément trouvé avec l'id {Id}", id);
                }
                else
                {
                    _logger.LogDebug("Aucun élément trouvé avec l'id {Id}", id);
                }

                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche de l'élément avec l'id {Id}", id);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<IEnumerable<XElement>> GetAllAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var elements = _document.Descendants().ToList();
                _logger.LogDebug("{Count} éléments trouvés", elements.Count);
                return elements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les éléments");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<XElement> AddAsync(XElement entity)
        {
            await _fileLock.WaitAsync();
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _document.Root.Add(entity);
                await SaveDocumentAsync();
                _logger.LogInformation("Nouvel élément ajouté avec l'id {Id}", entity.Attribute("id")?.Value);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout d'un nouvel élément");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<XElement> UpdateAsync(XElement entity)
        {
            await _fileLock.WaitAsync();
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var id = entity.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentException("L'élément doit avoir un attribut 'id'", nameof(entity));

                var existingElement = _document.Descendants()
                    .FirstOrDefault(e => e.Attribute("id")?.Value == id);
                if (existingElement == null)
                    throw new KeyNotFoundException($"Aucun élément trouvé avec l'id {id}");

                existingElement.ReplaceWith(entity);
                await SaveDocumentAsync();
                _logger.LogInformation("Élément mis à jour avec l'id {Id}", id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'élément");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var element = _document.Descendants()
                    .FirstOrDefault(e => e.Attribute("id")?.Value == id);
                if (element == null)
                {
                    _logger.LogWarning("Tentative de suppression d'un élément inexistant avec l'id {Id}", id);
                    return false;
                }

                element.Remove();
                await SaveDocumentAsync();
                _logger.LogInformation("Élément supprimé avec l'id {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'élément avec l'id {Id}", id);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var exists = _document.Descendants()
                    .Any(e => e.Attribute("id")?.Value == id);
                _logger.LogDebug("Vérification de l'existence de l'élément avec l'id {Id} : {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de l'élément avec l'id {Id}", id);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<int> CountAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var count = _document.Descendants().Count();
                _logger.LogDebug("Nombre d'éléments dans le document : {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du comptage des éléments");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
} 