﻿namespace Ezbob.Backend.Strategies.MainStrategyNew.Exceptions {
	using System;

	internal class CreateFindCashRequestException : Exception {
		public CreateFindCashRequestException(string format, params object[] args) : base(string.Format(format, args)) {
		} // constructor
	} // class CreateFindCashRequestException
} // namespace
