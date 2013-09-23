using System;

namespace YodleeLib
{
	class RefreshYodleeException : Exception
	{
		public RefreshStatus RefreshStatus { get; private set; }

		public RefreshYodleeException(RefreshStatus refreshStatus)
			: base(refreshStatus.ToString())
		{
			RefreshStatus = refreshStatus;
		}

		public override string ToString()
		{
			return RefreshStatus.ToString();
		}
	}
}
