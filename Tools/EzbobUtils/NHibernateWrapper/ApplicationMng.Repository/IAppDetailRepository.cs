using ApplicationMng.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public interface IAppDetailRepository : IRepository<AppDetail>, System.IDisposable
	{
		AppDetail GetDetailByName(string name, long appid);
		AppDetail GetAttachments(long appId);
		void ClearDetails(long appid, string mask, string[] excluded);
		System.Collections.Generic.IList<AppDetail> GetAppDetailsFiltered(long appId, string mask, string[] excluded);
		System.Collections.Generic.IList<AppDetail> GetAppDetailsWithChildren(long appId, string mask, string[] excluded);
		System.Collections.Generic.IList<AppDetail> GetAppDetailsCustom(long appId, string[] names);
	}
}
