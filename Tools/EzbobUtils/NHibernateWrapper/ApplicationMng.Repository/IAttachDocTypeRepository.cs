using ApplicationMng.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public interface IAttachDocTypeRepository : IRepository<AttachDocType>, System.IDisposable
	{
		System.Collections.Generic.IEnumerable<string> GetTypes(string groupName);
	}
}
