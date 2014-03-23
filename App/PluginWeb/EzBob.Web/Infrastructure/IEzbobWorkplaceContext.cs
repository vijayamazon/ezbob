using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Infrastructure {
	using NHibernateWrapper.Web;

	public interface IEzbobWorkplaceContext : IWorkplaceContext {
		Customer Customer { get; }
	}
}