using NHibernate;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public interface IRepository<T> : System.IDisposable
	{
		T Get(object id);
		System.Linq.IQueryable<T> GetAll();
		object Save(T val);
		void SaveOrUpdate(T val);
		void Update(T val);
		T Merge(T val);
		void Delete(T val);
		void BeginTransaction();
		void CommitTransaction();
		void EnsureTransaction(System.Action<ITransaction> action);
		void EnsureTransaction(System.Action action);
		void Clear();
		T Load(object id);
	}
}
