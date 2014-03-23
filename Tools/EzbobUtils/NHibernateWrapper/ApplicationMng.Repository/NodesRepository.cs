using ApplicationMng.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class NodesRepository : NHibernateRepositoryBase<Node>
	{
		public NodesRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IList<Node> GetByNamesWithGuids(params string[] names)
		{
			System.Collections.Generic.IList<Node> result;
			if (names == null || names.Length == 0)
			{
				result = new System.Collections.Generic.List<Node>();
			}
			else
			{
				System.Collections.Generic.IList<Node> list = this._session.CreateQuery("from ApplicationMng.Model.Node n where n.IsDeleted = 0 and (n.Name||n.Guid) in (:names)").SetParameterList("names", names).List<Node>();
				string[] array = names.Except(
					from t in list
					select t.Name + t.Guid).ToArray<string>();
				if (array.Length > 0)
				{
					string message = string.Format("Nodes '{0}' were not found.", string.Join(", ", array));
					throw new NodeNotFoundException(message);
				}
				result = list;
			}
			return result;
		}
		public Node GetNodeByName(string name)
		{
			return this.GetAll().Single((Node n) => n.Name == name && n.IsDeleted == (int?)0 && n.TerminationDate == (System.DateTime?)null);
		}
	}
}
