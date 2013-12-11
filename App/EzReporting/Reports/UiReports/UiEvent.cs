using System;
using System.Data;

namespace Reports {
	#region class UiEvent

	public class UiEvent : Extractor {
		#region public

		#region constructor

		public UiEvent(IDataRecord oRow) : base(oRow) {
			ControlID = Retrieve<int>("UiControlID").Value;
			ActionID = Retrieve<int>("UiActionID").Value;
			EventTime = Retrieve<DateTime>("EventTime").Value;
			UserID = Retrieve<int>("UserID").Value;
			ControlHtmlID = Retrieve("ControlHtmlID");
			EventArguments = Retrieve("EventArguments");
		} // constructor

		#endregion constructor

		#region properties

		public int ControlID { get; private set; }
		public int ActionID { get; private set; }
		public DateTime EventTime { get; private set; }
		public int UserID { get; private set; }
		public string ControlHtmlID { get; private set; }
		public string EventArguments { get; private set; }

		#endregion properties

		#region method ToString

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

		#endregion method ToString

		#endregion public
	} // class UiEvent

	#endregion class UiEvent
} // namespace Reports
