namespace EzBob.Web.Infrastructure {
	using EZBob.DatabaseLib.Model.Database;

	public interface IEzbobWorkplaceContext : IWorkplaceContext {
		Customer Customer { get; }
	} // interface IEzbobWorkplaceContext
} // namespace
