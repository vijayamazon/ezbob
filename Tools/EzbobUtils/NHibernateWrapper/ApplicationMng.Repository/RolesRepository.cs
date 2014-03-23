using ApplicationMng.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class RolesRepository : NHibernateRepositoryBase<Role>, IRolesRepository, IRepository<Role>, System.IDisposable
	{
		public RolesRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IList<Role> GetRolesByNames(System.Collections.Generic.IEnumerable<string> names)
		{
			return (
				from r in this.GetAll()
				where names.Contains(r.Name)
				select r).ToList<Role>();
		}
		public System.Collections.Generic.IList<Role> GetRolesByNameWithEmpty(System.Collections.Generic.IEnumerable<string> names)
		{
			System.Collections.Generic.List<Role> list = new System.Collections.Generic.List<Role>
			{
				new Role
				{
					Id = -1,
					Name = "<Ïóñòî>",
					Description = "<Ïóñòî>"
				}
			};
			list.AddRange(this.GetRolesByNames(names));
			return list;
		}
	}
}
