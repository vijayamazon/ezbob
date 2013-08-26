namespace EzAutoResponder
{
	using System.ServiceProcess;

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
            { 
                new AutoResponderService() 
            };
			ServiceBase.Run(ServicesToRun);
		}
	}
}
