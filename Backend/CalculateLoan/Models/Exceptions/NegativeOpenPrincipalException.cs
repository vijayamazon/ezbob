﻿namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeOpenPrincipalException : ANegativeDecimalException {
		public NegativeOpenPrincipalException(decimal principal) : base("Open principal is negative: {0}.", principal) {
		} // constructor
	} // class NegativeOpenPrincipalException
} // namespace
