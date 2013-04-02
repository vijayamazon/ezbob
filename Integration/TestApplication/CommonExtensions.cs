using System;
using System.Windows.Forms;

namespace TestApplication
{
	public static class CommonExtensions
	{
		public static void RaiseEvent( this object sender, EventHandler handler )
		{
			var h = handler;

			if ( h != null )
			{
				h( sender, EventArgs.Empty );
			}
		}

		public static void InvokeIfNeeded( this Control control, Action a )
		{
			if ( control.InvokeRequired )
			{
				control.Invoke( new MethodInvoker( () => a() ) );
			}
			else
			{
				a();
			}
		}
	}
}