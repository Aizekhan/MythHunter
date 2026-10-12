// Assets/_MythHunter/Code/Core/Initialization/IInitializationTransaction.cs
using System;

namespace MythHunter.Core.Initialization
{
    /// <summary>
    /// EPIC 08 Phase 4: Interface for initialization transaction
    /// </summary>
    public interface IInitializationTransaction : IDisposable
    {
        /// <summary>
        /// Registers an action to be called if rollback is needed
        /// </summary>
        void RegisterRollback(Action rollbackAction, string description = null);

        /// <summary>
        /// Commits the transaction - no rollback will occur
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back all registered actions in reverse order
        /// </summary>
        void Rollback();
    }
}
