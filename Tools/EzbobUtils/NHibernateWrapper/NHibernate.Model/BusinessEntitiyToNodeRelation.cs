using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class BusinessEntitiyToNodeRelation
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual Node Node
		{
			get;
			set;
		}
		public virtual BusinessEntity BusinessEntity
		{
			get;
			set;
		}
	}
}
