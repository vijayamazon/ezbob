using ApplicationMng.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public interface IPublicNameRepository : IRepository<PublicName>, System.IDisposable
	{
		System.Linq.IQueryable<PublicName> GetAllPublicNames();
		System.Linq.IQueryable<PublicName> GetActivePublicNames();
		System.Collections.Generic.IList<PublicName> GetActivePublicNamesList();
	}
}
