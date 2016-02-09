namespace ApplicationMng.Repository {
	using System;
	using NHibernate;
	using NHibernate.Linq;

	public class NHibernateRepositoryBase<T> : IRepository<T> where T : class {
		public NHibernateRepositoryBase(ISession session) {
			this.Session = session;
		} // constructor

		public virtual T Get(object id) {
			return this.Session.Get<T>(id);
		} // Get

		public virtual System.Linq.IQueryable<T> GetAll() {
			return this.Session.Query<T>();
		} // GetAll

		public virtual object Save(T val) {
			object result = null;
			this.EnsureTransaction(() => { result = this.Session.Save(val); });
			return result;
		} // Save

		public virtual void SaveOrUpdate(T val) {
			this.EnsureTransaction(() => {
				this.Session.SaveOrUpdate(val);
			});
		} // SaveOrUpdate

		public virtual void Update(T val) {
			this.EnsureTransaction(() => { this.Session.Update(val); });
		} // Update

		public T Merge(T val) {
			T merged = default(T);
			this.EnsureTransaction<T>(() => merged = this.Session.Merge<T>(val));
			return merged;
		} // Merge

		public virtual void Delete(T val) {
			this.EnsureTransaction(() => { this.Session.Delete(val); });
		} // Delete

		public virtual void BeginTransaction() {
			this.transaction = this.Session.BeginTransaction();
		} // BeginTransaction

		public virtual void CommitTransaction() {
			this.transaction.Commit();
		} // CommitTransaction

		public virtual void RollbackTransaction() {
			try {
				this.transaction.Rollback();
			} finally {
				this.transaction = null;	
			}
		} // RollbackTransaction

		public virtual void EvictAll() {
			this.Session.SessionFactory.Evict(typeof(T));
		} // EvictAll

		public void Clear() {
			this.Session.Clear();
		} // Clear

		public T Load(object id) {
			return this.Session.Load<T>(id);
		} // Load

		/// <summary>
		/// evict object
		/// </summary>
		/// <param name="val"></param>
		public void Evict(T val) {
			this.Session.Evict(val);
		} // Evict

		public virtual void EnsureTransaction(System.Action action) {
			this.EnsureTransaction(delegate(ITransaction transaction) { action(); });
		} // EnsureTransaction

		public virtual RT EnsureTransaction<RT>(System.Func<RT> action) {
			return this.EnsureTransaction<RT>(action, System.Data.IsolationLevel.Unspecified);
		} // EnsureTransaction

		public virtual RT EnsureTransaction<RT>(System.Func<RT> action, System.Data.IsolationLevel isolationLevel) {
			RT r = default(RT);
			this.EnsureTransaction(delegate(ITransaction transaction) { r = action(); }, isolationLevel);
			return r;
		} // EnsureTransaction

		public virtual void EnsureTransaction(System.Action<ITransaction> action) {
			this.EnsureTransaction(action, System.Data.IsolationLevel.Unspecified);
		} // EnsureTransaction

		public virtual void EnsureTransaction(System.Action<ITransaction> action, System.Data.IsolationLevel isolationLevel) {
			ITransaction transaction = null;

			try {
				if (this.Session.Transaction == null || !this.Session.Transaction.IsActive) {
					transaction = this.Session.BeginTransaction(isolationLevel);
					action(transaction);
					transaction.Commit();
				} else
					action(this.Session.Transaction);
			} catch (System.Exception) {
				if (transaction != null) {
					if (transaction.IsActive)
						transaction.Rollback();

					transaction.Dispose();
					transaction = null;
				} // if

				throw;
			} finally {
				if (transaction != null)
					transaction.Dispose();
			} // try
		} // EnsureTransaction

		public void Dispose() {
			try {
				if (this.transaction != null) {
					this.transaction.Rollback();
					this.transaction.Dispose();
				} // if
			} finally {
				this.transaction = null;
			}
		} // Dispose

		protected ISession Session { get; private set; }
		private ITransaction transaction;
	} // class NHibernateRepositoryBase
} // namespace
