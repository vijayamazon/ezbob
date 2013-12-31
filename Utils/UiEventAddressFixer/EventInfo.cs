namespace UiEventAddressFixer {
	using System;
	using System.Data.Common;

	class EventInfo {
		public EventInfo(DbDataReader oRow) {
			EventID = Convert.ToInt64(oRow["UiEventID"]);
			UserID = Convert.ToInt32(oRow["UserID"]);
			ControlID = Convert.ToInt32(oRow["UiControlID"]);
			ControlName = oRow["UiControlName"].ToString();
			Args = oRow["EventArguments"].ToString();
		} // constructor

		public long EventID { get; private set; }
		public int UserID { get; private set; }
		public int ControlID { get; private set; }
		public string ControlName { get; private set; }
		public string Args { get; private set; }

		public override string ToString() {
			return string.Format(
				"{0}: {1} - {2}({3}) - {4}",
				EventID, UserID, ControlName, ControlID, Args
			);
		} // ToString
	} // class EventInfo
} // namespace UiEventAddressFixer
