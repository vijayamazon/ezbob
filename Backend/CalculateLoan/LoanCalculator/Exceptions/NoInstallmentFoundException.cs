﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;

	public class NoInstallmentFoundException : ACalculateLoanException {
		public NoInstallmentFoundException(DateTime eventDate)
			: base(string.Format("Nearby schedule item for event date {0} not found.", eventDate.Date)){
		} 
	} 
} 
