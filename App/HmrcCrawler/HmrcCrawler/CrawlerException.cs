using System;

namespace Ezbob.HmrcCrawler {
	#region class CrawlerException

	class CrawlerException : Exception {
		#region public

		#region constructor

		public CrawlerException(string sMsg) : base(sMsg) {} // constructor

		public CrawlerException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

		#endregion constructor

		#endregion public
	} // class CrawlerException

	#endregion class CrawlerException
} // namespace Ezbob.HmrcCrawler
