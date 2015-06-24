namespace Reports.UiReports {
	using System;
	using System.Data;

	public class UiEvent : Extractor {
		public UiEvent(IDataRecord oRow) : base(oRow) {
			ControlID = Retrieve<int>("UiControlID").Value;
			ActionID = Retrieve<int>("UiActionID").Value;
			EventTime = Retrieve<DateTime>("EventTime").Value;
			UserID = Retrieve<int>("UserID").Value;
			ControlHtmlID = Retrieve("ControlHtmlID");
			EventArguments = Retrieve("EventArguments");
		} // constructor

		public int ControlID { get; private set; }
		public int ActionID { get; private set; }
		public DateTime EventTime { get; private set; }
		public int UserID { get; private set; }
		public string ControlHtmlID { get; private set; }
		public string EventArguments { get; private set; }

		public override string ToString() {
			return string.Format("{0} has done {1} on {2} {3} at {4}, args {5}",
				UserID,
				ActionID,
				ControlID,
				Value(ControlHtmlID),
				Value(EventTime),
				Value(EventArguments)
			);
		} // ToString
	} // class UiEvent
} // namespace
