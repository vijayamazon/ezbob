using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class BusinessEntitiyToStrategyRelation
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual Strategy Strategy
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
