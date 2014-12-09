namespace EzWinServiceHost
{
	using System.ComponentModel;
	using System.Configuration.Install;
	using System.ServiceProcess;

	[RunInstaller(true)]
	public partial class EzWinServiceInstaller : Installer
	{
		public EzWinServiceInstaller()
		{
			ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
			ServiceInstaller serviceInstaller = new ServiceInstaller();

			//# Service Account Information
			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;

			//# Service Information
			serviceInstaller.DisplayName = "Ezbob Service";
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.Description = "Ezbob Service.";

			//# This must be identical to the WindowsService.ServiceBase name
			//# set in the constructor of WindowsService.cs
			serviceInstaller.ServiceName = "EzWinService";
			serviceInstaller.AfterInstall += ServiceInstaller_AfterInstall;
			this.Installers.Add(serviceProcessInstaller);
			this.Installers.Add(serviceInstaller);

		}
		private void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
		{
			ServiceController sc = new ServiceController("EzWinService");
			sc.Start();
		}

	}
}
