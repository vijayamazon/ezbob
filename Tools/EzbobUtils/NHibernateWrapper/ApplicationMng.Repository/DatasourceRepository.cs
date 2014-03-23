using ApplicationMng.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class DatasourceRepository : NHibernateRepositoryBase<DataSource>
	{
		public DatasourceRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IList<DataSource> GetByNamesWithGuids(params string[] names)
		{
			System.Collections.Generic.IList<DataSource> result;
			if (names == null || names.Length == 0)
			{
				result = new System.Collections.Generic.List<DataSource>();
			}
			else
			{
				System.Collections.Generic.IList<DataSource> list = this._session.CreateQuery("from ApplicationMng.Model.DataSource n where n.IsDeleted = 0 and (n.Name) in (:names)").SetParameterList("names", names).List<DataSource>();
				string[] array = names.Except(
					from t in list
					select t.Name).ToArray<string>();
				if (array.Length > 0)
				{
					string message = string.Format("Datasource '{0}' were not found.", string.Join(", ", array));
					throw new DatasourceNotFoundException(message);
				}
				result = list;
			}
			return result;
		}
		public DataSource GetNodeByName(string name)
		{
			return this.GetAll().Single((DataSource n) => n.Name == name && n.IsDeleted == (int?)0 && n.TerminationDate == (System.DateTime?)null);
		}
	}
}
