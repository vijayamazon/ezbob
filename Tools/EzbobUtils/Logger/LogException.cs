﻿namespace Ezbob.Logger {
	using System;

	public class LogException : Exception {
		public LogException(string sMsg) : base(sMsg) {} // constructor
		public LogException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor
	} // class LogException
} // namespace Logger
