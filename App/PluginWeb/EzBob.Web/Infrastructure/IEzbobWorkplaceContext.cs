using EZBob.DatabaseLib.Model.Database;
using Scorto.Web;

namespace EzBob.Web.Infrastructure {
	public interface IEzbobWorkplaceContext : IWorkplaceContext {
		Customer Customer { get; }
	}
}