namespace EzBob.TeraPeakServiceLib
{
	public abstract class TeraPeakRequester
	{
		protected ITeraPeakService Service { get; private set; }

		protected TeraPeakRequester(ITeraPeakService service)
		{
			Service = service;
		}
	}
}