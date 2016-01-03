namespace EzBobPersistence {
    using System;
    using System.Transactions;

    /// <summary>
    /// Implements unit of work with transaction scope. 
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork {
        [ThreadStatic]
        private static TransactionScope transactionScope;

        public UnitOfWork(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            if (Transaction.Current != null) {
                return;//if we already a part of transaction do nothing
            }
            BeginUnitOfWork(isolationLevel);
        }
        /// <summary>
        /// Begins the unit of work.
        /// </summary>
        private void BeginUnitOfWork(IsolationLevel isolationLevel) {
            if (transactionScope == null) {
                var transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = isolationLevel;
//                transactionOptions.Timeout = TimeSpan.FromMinutes(5);
                transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
            }
        }

        /// <summary>
        /// Commits the unit of work
        /// </summary>
        public void Commit() {
            if (transactionScope != null) {
                transactionScope.Complete();
            }
            Dispose();
        }

        /// <summary>
        /// rollbacks unit of work
        /// </summary>
        private static void Dispose() {
            if (transactionScope != null) {
                transactionScope.Dispose();
                transactionScope = null;
            }
        }

        /// <summary>
        /// rollbacks unit of work
        /// </summary>
        void IDisposable.Dispose() {
            Dispose();
        }
    }
}
