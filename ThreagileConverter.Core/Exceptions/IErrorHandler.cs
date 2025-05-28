using System;
using System.Threading.Tasks;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Interface for error handlers
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Registers an error handler
        /// </summary>
        /// <param name="handler">The error handler</param>
        void RegisterErrorHandler(Action<ThreagileConverterException> handler);

        /// <summary>
        /// Handles an exception
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        void HandleException(Exception exception);

        /// <summary>
        /// Executes an action and handles any exceptions
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>True if the action completed successfully, false otherwise</returns>
        bool TryExecute(Action action);

        /// <summary>
        /// Executes a function and handles any exceptions
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="defaultValue">The default value to return if an exception occurs</param>
        /// <returns>The result of the function or the default value if an exception occurs</returns>
        T TryExecute<T>(Func<T> func, T defaultValue);

        /// <summary>
        /// Executes an async function and handles any exceptions
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The async function to execute</param>
        /// <param name="defaultValue">The default value to return if an exception occurs</param>
        /// <returns>The result of the function or the default value if an exception occurs</returns>
        Task<T> TryExecuteAsync<T>(Func<Task<T>> func, T defaultValue);

        /// <summary>
        /// Executes an async action and handles any exceptions
        /// </summary>
        /// <param name="action">The async action to execute</param>
        /// <returns>True if the action completed successfully, false otherwise</returns>
        Task<bool> TryExecuteAsync(Func<Task> action);
    }
} 