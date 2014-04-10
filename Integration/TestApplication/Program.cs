using System;
using log4net.Config;
using System.Windows.Forms;

[assembly: XmlConfigurator(ConfigFileExtension = "log4net", Watch = true)]
namespace TestApplication
{
	using Ezbob.RegistryScanner;

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var log = log4net.LogManager.GetLogger( "TestApp" );
			try
			{				
				log.Info( "started" );
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault( false );
				Scanner.Register();
				Application.Run( new Form1() );
			}
			finally
			{
				log.Info( "ended" );
			}
		}
	}
}
