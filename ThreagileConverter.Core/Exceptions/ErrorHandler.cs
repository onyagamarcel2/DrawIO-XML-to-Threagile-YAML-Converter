using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Provides centralized error handling for the application
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger<ErrorHandler> _logger;
        private readonly List<Action<ThreagileConverterException>> _errorHandlers;

        /// <summary>
        /// Creates a new instance of ErrorHandler
        /// </summary>
        /// <param name="logger">The logger</param>
        public ErrorHandler(ILogger<ErrorHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandlers = new List<Action<ThreagileConverterException>>();
        }

        /// <summary>
        /// Registers an error handler
        /// </summary>
        /// <param name="handler">The error handler</param>
        public void RegisterErrorHandler(Action<ThreagileConverterException> handler)
        {
            _errorHandlers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
        }

        /// <summary>
        /// Handles an exception
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        public void HandleException(Exception exception)
        {
            if (exception == null)
                return;

            // Log the exception
            LogException(exception);

            // Convert to ThreagileConverterException if needed
            var threagileException = ConvertException(exception);

            // Notify registered handlers
            foreach (var handler in _errorHandlers)
            {
                try
                {
                    handler(threagileException);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in error handler");
                }
            }
        }

        /// <summary>
        /// Executes an action and handles any exceptions
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>True if the action completed successfully, false otherwise</returns>
        public bool TryExecute(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }

        /// <summary>
        /// Executes a function and handles any exceptions
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="defaultValue">The default value to return if an exception occurs</param>
        /// <returns>The result of the function or the default value if an exception occurs</returns>
        public T TryExecute<T>(Func<T> func, T defaultValue)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// Executes an async function and handles any exceptions
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The async function to execute</param>
        /// <param name="defaultValue">The default value to return if an exception occurs</param>
        /// <returns>The result of the function or the default value if an exception occurs</returns>
        public async Task<T> TryExecuteAsync<T>(Func<Task<T>> func, T defaultValue)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// Executes an async action and handles any exceptions
        /// </summary>
        /// <param name="action">The async action to execute</param>
        /// <returns>True if the action completed successfully, false otherwise</returns>
        public async Task<bool> TryExecuteAsync(Func<Task> action)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }

        private void LogException(Exception exception)
        {
            if (exception is ThreagileConverterException threagileEx)
            {
                switch (threagileEx.Severity)
                {
                    case ErrorSeverity.Info:
                        _logger.LogInformation(exception, "[{ErrorCode}] {Message}", threagileEx.ErrorCode, exception.Message);
                        break;
                    case ErrorSeverity.Warning:
                        _logger.LogWarning(exception, "[{ErrorCode}] {Message}", threagileEx.ErrorCode, exception.Message);
                        break;
                    case ErrorSeverity.Critical:
                        _logger.LogCritical(exception, "[{ErrorCode}] {Message}", threagileEx.ErrorCode, exception.Message);
                        break;
                    default:
                        _logger.LogError(exception, "[{ErrorCode}] {Message}", threagileEx.ErrorCode, exception.Message);
                        break;
                }
            }
            else
            {
                _logger.LogError(exception, "{Message}", exception.Message);
            }
        }

        private ThreagileConverterException ConvertException(Exception exception)
        {
            if (exception is ThreagileConverterException threagileEx)
                return threagileEx;

            // Map standard exceptions to our custom exceptions
            if (exception is ArgumentNullException argNullEx)
                return new ValidationException(argNullEx.Message, ValidationErrorType.MissingRequiredField, argNullEx);
            
            if (exception is ArgumentException argEx)
                return new ValidationException(argEx.Message, ValidationErrorType.InvalidFieldValue, argEx);
            
            if (exception is InvalidOperationException invOpEx)
                return new ThreagileConverterException(invOpEx.Message, "INVALID_OPERATION", invOpEx);
            
            if (exception is NotSupportedException notSupEx)
                return new ThreagileConverterException(notSupEx.Message, "NOT_SUPPORTED", notSupEx);
            
            if (exception is TimeoutException timeoutEx)
                return new ThreagileConverterException(timeoutEx.Message, "TIMEOUT", ErrorSeverity.Warning, timeoutEx);
            
            if (exception is System.IO.IOException ioEx)
                return new GenerationException(ioEx.Message, GenerationErrorType.AccessDenied, ioEx);
            
            if (exception is System.IO.FileNotFoundException fileNotFoundEx)
                return new ParsingException(fileNotFoundEx.Message, ParsingErrorType.FileNotFound, fileNotFoundEx.FileName, fileNotFoundEx);
            
            // Default case
            return new ThreagileConverterException(exception.Message, "UNHANDLED_ERROR", exception);
        }
    }
} 