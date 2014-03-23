using ApplicationMng.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public interface IRolesRepository : IRepository<Role>, System.IDisposable
	{
		System.Collections.Generic.IList<Role> GetRolesByNames(System.Collections.Generic.IEnumerable<string> names);
		System.Collections.Generic.IList<Role> GetRolesByNameWithEmpty(System.Collections.Generic.IEnumerable<string> names);
	}
}
