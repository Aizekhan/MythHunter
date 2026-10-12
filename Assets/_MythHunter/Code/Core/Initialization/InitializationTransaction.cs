// Assets/_MythHunter/Code/Core/Initialization/InitializationTransaction.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Initialization
{
    /// <summary>
    /// EPIC 08 Phase 4: Transactional initialization with rollback support
    /// Allows rollback of partially completed initialization on failure
    /// </summary>
    public class InitializationTransaction : IDisposable
    {
        private readonly Stack<Action> _rollbackActions = new Stack<Action>();
        private readonly IMythLogger _logger;
        private bool _committed = false;
        private bool _disposed = false;

        public InitializationTransaction(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registers an action to be called if rollback is needed
        /// </summary>
        public void RegisterRollback(Action rollbackAction, string description = null)
        {
            if (_committed)
                throw new InvalidOperationException("Cannot register rollback after commit");

            if (rollbackAction == null)
                throw new ArgumentNullException(nameof(rollbackAction));

            _rollbackActions.Push(rollbackAction);

            if (!string.IsNullOrEmpty(description))
            {
                _logger?.LogDebug($"Registered rollback: {description}", "InitTransaction");
            }
        }

        /// <summary>
        /// Commits the transaction - no rollback will occur
        /// </summary>
        public void Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InitializationTransaction));

            _committed = true;
            _rollbackActions.Clear();
            _logger?.LogInfo("Initialization transaction committed", "InitTransaction");
        }

        /// <summary>
        /// Rolls back all registered actions in reverse order (LIFO)
        /// </summary>
        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InitializationTransaction));

            if (_committed)
            {
                _logger?.LogWarning("Attempted rollback on committed transaction", "InitTransaction");
                return;
            }

            _logger?.LogWarning($"Rolling back {_rollbackActions.Count} initialization steps", "InitTransaction");

            int successCount = 0;
            int failCount = 0;

            while (_rollbackActions.Count > 0)
            {
                var rollbackAction = _rollbackActions.Pop();
                try
                {
                    rollbackAction();
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger?.LogError($"Error during rollback step: {ex.Message}", "InitTransaction", ex);
                }
            }

            _logger?.LogInfo($"Rollback complete: {successCount} succeeded, {failCount} failed", "InitTransaction");
        }

        /// <summary>
        /// Dispose automatically rolls back if not committed
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_committed && _rollbackActions.Count > 0)
            {
                _logger?.LogWarning("Transaction disposed without commit - rolling back", "InitTransaction");
                Rollback();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Helper extensions for common initialization patterns
    /// </summary>
    public static class InitializationTransactionExtensions
    {
        /// <summary>
        /// Register disposable cleanup on rollback
        /// </summary>
        public static void RegisterDisposableRollback(this InitializationTransaction transaction,
            IDisposable disposable, string description = null)
        {
            if (disposable != null)
            {
                transaction.RegisterRollback(() => disposable.Dispose(), description);
            }
        }

        /// <summary>
        /// Register cleanup action with error handling
        /// </summary>
        public static void RegisterSafeRollback(this InitializationTransaction transaction,
            Action rollbackAction, string description, IMythLogger logger)
        {
            transaction.RegisterRollback(() =>
            {
                try
                {
                    rollbackAction();
                }
                catch (Exception ex)
                {
                    logger?.LogError($"Error in {description} rollback: {ex.Message}", "InitTransaction", ex);
                }
            }, description);
        }
    }
}
