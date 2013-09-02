using System;

namespace PacnetBalance
{
	public class PacNetBalanceException : Exception
	{
		private readonly String message = null;

		/// <summary>
		/// Gets error message
		/// </summary>
		public override String Message
		{
			get { return this.message; }
		}

		public PacNetBalanceException(string message)
		{
			this.message = message;
		}

		public PacNetBalanceException(Exception ex) : this(ex.Message)
		{}
	}
}
