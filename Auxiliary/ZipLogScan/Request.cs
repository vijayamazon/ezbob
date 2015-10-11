namespace ZipLogScan {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	class Request {
		public Request(char requestType, DateTime time, ASafeLog log) {
			switch (requestType) {
			case 'N':
				RequestType = RequestTypes.NonLimited;
				break;

			case 'L':
				RequestType = RequestTypes.Limited;
				break;

			case 'B':
				RequestType = RequestTypes.Targeting;
				break;
			} // switch

			Time = time;

			log.Debug("New request created: {0}", this);
		} // constructor

		public Request(RequestTypes requestType, DateTime time, ASafeLog log) {
			RequestType = requestType;
			Time = time;

			log.Debug("New request created: {0}", this);
		} // constructor

		public RequestTypes RequestType { get; private set; }
		public DateTime Time { get; private set; }

		public string Month {
			get { return Time.ToString("yyyy-MM", CultureInfo.InvariantCulture); }
		} // Month

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("{0} at {1}", RequestType, Time.ToString("d/MMM/yyyy"));
		} // ToString
	} // class Request
} // namespace
