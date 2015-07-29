namespace Ezbob.Backend.Strategies.Exceptions{
	using System;
	public class PacnetMailProcessorException : Exception{
		private readonly String message;

		/// <summary>
		/// Gets error message
		/// </summary>
		public override String Message{
			get { return this.message; }
		}

		public PacnetMailProcessorException(string message){
			this.message = message;
		}

		public PacnetMailProcessorException(Exception ex) : this(ex.Message){}
	}
}
