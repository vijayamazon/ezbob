using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports {
	#region class UiReportItemGroupData

	public class UiReportItemGroupData {
		#region public

		#region constructor

		public UiReportItemGroupData(SortedSet<int> oRelevantControls) {
			m_oRelevantControls = oRelevantControls;
		} // constructor

		#endregion constructor

		public UiReportItemGroupAction Action { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		#region method Generate

		public void Generate() {
			// TODO
		} // Generate

		#endregion method Generate

		#region method AddEvent

		public void AddEvent(UiEvent oEvent) {
			if (!m_oRelevantControls.Contains(oEvent.ControlID))
				return;

			// TODO
		} // AddEvent

		#endregion method AddEvent

		#endregion public

		#region private

		private readonly SortedSet<int> m_oRelevantControls;

		#endregion private
	} // class UiReportItemGroupData

	#endregion class UiReportItemGroupData
} // namespace Reports
