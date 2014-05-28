namespace EzBob.Web.Infrastructure.Hmrc {
	using System;

	public class HmrcManualAccountManagerException : Exception {
		public HmrcManualAccountManagerException(string sMsg) : base(sMsg) {} // constructor

		public HmrcManualAccountManagerException(Exception oInner) : base(oInner.Message, oInner) {} // constructor
	} // class HmrcManualAccountManagerException
} // namespace
