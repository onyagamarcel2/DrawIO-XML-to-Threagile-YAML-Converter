using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreagileConverter.Core.Repositories
{
    /// <summary>
    /// Interface générique définissant les opérations CRUD de base pour un repository
    /// </summary>
    /// <typeparam name="T">Type de l'entité gérée par le repository</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Récupère une entité par son identifiant
        /// </summary>
        /// <param name="id">Identifiant de l'entité</param>
        /// <returns>L'entité trouvée ou null si non trouvée</returns>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Récupère toutes les entités
        /// </summary>
        /// <returns>Collection d'entités</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Ajoute une nouvelle entité
        /// </summary>
        /// <param name="entity">Entité à ajouter</param>
        /// <returns>L'entité ajoutée avec son identifiant</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Met à jour une entité existante
        /// </summary>
        /// <param name="entity">Entité à mettre à jour</param>
        /// <returns>L'entité mise à jour</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Supprime une entité par son identifiant
        /// </summary>
        /// <param name="id">Identifiant de l'entité à supprimer</param>
        /// <returns>True si la suppression a réussi, False sinon</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Vérifie si une entité existe par son identifiant
        /// </summary>
        /// <param name="id">Identifiant de l'entité</param>
        /// <returns>True si l'entité existe, False sinon</returns>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Compte le nombre total d'entités
        /// </summary>
        /// <returns>Nombre d'entités</returns>
        Task<int> CountAsync();
    }
} 