using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Infrastructure {
	public interface IEzbobWorkplaceContext : IWorkplaceContext {
		Customer Customer { get; }
	}
}