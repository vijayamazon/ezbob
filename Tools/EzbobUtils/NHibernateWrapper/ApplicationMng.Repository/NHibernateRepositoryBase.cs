namespace ApplicationMng.Repository
{
	using NHibernate;
	using NHibernate.Linq;

	public class NHibernateRepositoryBase<T> : IRepository<T> where T : class
	{
		protected ISession _session;
		protected ITransaction _tx;
		public NHibernateRepositoryBase(ISession session)
		{
			this._session = session;
		}
		public virtual T Get(object id)
		{
			return this._session.Get<T>(id);
		}
		public virtual System.Linq.IQueryable<T> GetAll()
		{
			return this._session.Query<T>();
		}
		public virtual object Save(T val)
		{
			object result = null;
			this.EnsureTransaction(() =>
			{
				result = this._session.Save(val);
			});
			return result;
		}
		public virtual void SaveOrUpdate(T val)
		{
			this.EnsureTransaction(() =>
			{
				this._session.SaveOrUpdate(val);
			});
		}
		public virtual void Update(T val)
		{
			this.EnsureTransaction(() =>
			{
				this._session.Update(val);
			});
		}
		public T Merge(T val)
		{
			T merged = default(T);
			this.EnsureTransaction<T>(() => merged = this._session.Merge<T>(val));
			return merged;
		}
		public virtual void Delete(T val)
		{
			this.EnsureTransaction(() =>
			{
				this._session.Delete(val);
			});
		}
		public virtual void BeginTransaction()
		{
			this._tx = this._session.BeginTransaction();
		}
		public virtual void CommitTransaction()
		{
			this._tx.Commit();
		}
		public virtual void RollbackTransaction()
		{
			this._tx.Rollback();
			this._tx = null;
		}
		public virtual void EvictAll()
		{
			this._session.SessionFactory.Evict(typeof(T));
		}
		public void Clear()
		{
			this._session.Clear();
		}
		public T Load(object id)
		{
			return this._session.Load<T>(id);
		}
		public virtual void EnsureTransaction(System.Action action)
		{
			this.EnsureTransaction(delegate(ITransaction transaction)
			{
				action();
			});
		}
		public virtual RT EnsureTransaction<RT>(System.Func<RT> action)
		{
			return this.EnsureTransaction<RT>(action, System.Data.IsolationLevel.Unspecified);
		}
		public virtual RT EnsureTransaction<RT>(System.Func<RT> action, System.Data.IsolationLevel isolationLevel)
		{
			RT r = default(RT);
			this.EnsureTransaction(delegate(ITransaction transaction)
			{
				r = action();
			}, isolationLevel);
			return r;
		}
		public virtual void EnsureTransaction(System.Action<ITransaction> action)
		{
			this.EnsureTransaction(action, System.Data.IsolationLevel.Unspecified);
		}
		public virtual void EnsureTransaction(System.Action<ITransaction> action, System.Data.IsolationLevel isolationLevel)
		{
			ITransaction transaction = null;
			try
			{
				if (this._session.Transaction == null || !this._session.Transaction.IsActive)
				{
					transaction = this._session.BeginTransaction(isolationLevel);
					action(transaction);
					transaction.Commit();
				}
				else
				{
					action(this._session.Transaction);
				}
			}
			catch (System.Exception)
			{
				if (transaction != null)
				{
					if (transaction.IsActive)
					{
						transaction.Rollback();
					}
					transaction.Dispose();
					transaction = null;
				}
				throw;
			}
			finally
			{
				if (transaction != null)
				{
					transaction.Dispose();
				}
			}
		}
		public void Dispose()
		{
			if (this._tx != null)
			{
				this._tx.Rollback();
				this._tx.Dispose();
				this._tx = null;
			}
		}
	}
}
