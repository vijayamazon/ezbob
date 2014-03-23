using ApplicationMng.Model;
using NHibernate;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public class AppDetailRepository : NHibernateRepositoryBase<AppDetail>, IAppDetailRepository, IRepository<AppDetail>, System.IDisposable
	{
		public AppDetailRepository(ISession session) : base(session)
		{
		}
		public void ClearDetails(long appId, string mask, string[] excluded)
		{
			this.EnsureTransaction<int>(() => this._session.CreateQuery("update ApplicationMng.Model.AppDetail as d set d.ValueStr = '' where d.App = :appid and d.Name.id in (select dn.id from ApplicationMng.Model.AppDetailName dn where dn.Name like :name and dn.Name not in (:excluded) ) ").SetString("name", mask).SetParameterList("excluded", excluded).SetInt64("appid", appId).ExecuteUpdate());
		}
		public AppDetail GetDetailByName(string name, long id)
		{
			return this._session.CreateQuery("from ApplicationMng.Model.AppDetail det where det.App.Id = :appid and det.Parent.Name.Name = 'Body' and det.Name.Name = :name").SetString("name", name).SetInt64("appid", id).UniqueResult<AppDetail>();
		}
		public AppDetail GetAttachments(long appId)
		{
			return this._session.CreateQuery("from ApplicationMng.Model.AppDetail det where det.App.Id = :appid and det.Parent.Name.Name = 'Root' and det.Name.Name = 'Attachments'").SetInt64("appid", appId).UniqueResult<AppDetail>();
		}
		public System.Collections.Generic.IList<AppDetail> GetAppDetailsFiltered(long appId, string mask, string[] excluded)
		{
			return this._session.CreateQuery("from ApplicationMng.Model.AppDetail as d where d.App = :appid and d.Parent.Name.Name = 'Body' and d.Name.id in (select dn.id from ApplicationMng.Model.AppDetailName dn where dn.Name like :name and dn.Name not in (:excluded) ) ").SetString("name", mask).SetParameterList("excluded", excluded).SetInt64("appid", appId).List<AppDetail>();
		}
		public System.Collections.Generic.IList<AppDetail> GetAppDetailsWithChildren(long appId, string mask, string[] excluded)
		{
			return this._session.CreateQuery("select d from\r\n                      ApplicationMng.Model.AppDetail as d\r\n                     ,ApplicationMng.Model.AppDetail d1\r\n                    where \r\n                        d1.Parent.Id = d.Id\r\n                        and d.App = :appid \r\n                        and (d.Parent.Name.Name = 'Body' or d.Parent.Name.Name = 'Attachments')\r\n                        and d.Name.id in (\r\n                            select dn.id\r\n                            from ApplicationMng.Model.AppDetailName dn \r\n                            where dn.Name like :name and dn.Name not in (:excluded) ) ").SetString("name", mask).SetParameterList("excluded", excluded).SetInt64("appid", appId).List<AppDetail>();
		}
		public System.Collections.Generic.IList<AppDetail> GetAppDetailsCustom(long appId, string[] names)
		{
			return this._session.CreateQuery("from ApplicationMng.Model.AppDetail det where det.App.Id = :appid and det.Parent.Name.Name = 'Body' and det.Name.Name in (:names)").SetParameterList("names", names).SetInt64("appid", appId).List<AppDetail>();
		}
	}
}
