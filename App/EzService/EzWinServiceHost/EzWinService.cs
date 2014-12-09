using System;
using System.ServiceProcess;
using System.Timers;

//manual http://conceptf1.blogspot.co.il/2013/11/create-windows-service-step-by-step.html
namespace EzWinServiceHost
{
	partial class EzWinService : ServiceBase
	{
		private Timer _timer = null;
		public EzWinService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				EventLog.WriteEntry("EzServiceHost started : " + DateTime.Now);
				_timer = new Timer();
				_timer.Interval = 10000; //in milliseconds

				EventLog.WriteEntry("Timer Interval is : " + _timer.Interval);

				// attach a function to elapsed event whenever time interval occur it will call this function
				_timer.Elapsed += timer_Elapsed;

				_timer.Start();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry("EzServiceHost error : " + ex);
			}
		}

		protected override void OnStop()
		{
			EventLog.WriteEntry("EzServiceHost Stopped : " + DateTime.Now);
		}

		protected void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				_timer.Stop();
				//perform your action here 
				// It will be occur on each time elapsed
			}
			catch
			{
				// Silently ignore for now.
			}
			finally
			{
				//restart the timer
				_timer.Start();
			}
		}

	}
}
