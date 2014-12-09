using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Logger;
using HtmlAgilityPack;

namespace Ezbob.HmrcHarvester {

	/// <summary>
	/// See DL-parser.png for logic.
	/// </summary>
	class DLParser : SafeLog {

		public DLParser(HtmlNode oDL, bool bFailOnEmptyDT = true, bool bVerboseLogging = false, ASafeLog oLog = null) : base(oLog) {
			Data = new SortedDictionary<string, string>();
			Success = false;

			m_nCurState = FsmState.A;
			m_oCurChild = null;
			m_sCurKey = null;

			VerboseLogging = bVerboseLogging;

			Parse(oDL, bFailOnEmptyDT);
		} // constructor

		public SortedDictionary<string, string> Data { get; private set; }

		public bool Success { get; private set; }

		private bool VerboseLogging { get; set; }

		private void Parse(HtmlNode oDL, bool bFailOnEmptyDT) {
			Debug("DL parser started, fail on empty DT: {0}", bFailOnEmptyDT ? "yes" : "no");

			while (m_nCurState != FsmState.I) {
				if (VerboseLogging) {
					Debug(
						"State: {0}, Current Key: {1}, Current Child Tag Name: {2}",
						m_nCurState,
						(m_sCurKey ?? "-- null --"),
						m_oCurChild == null ? "-- null --" : m_oCurChild.Name
					);
				} // if

				switch (m_nCurState) {
				case FsmState.A:
					Data.Clear();
					Success = false;

					m_nCurState = oDL.FirstChild == null ? FsmState.I : FsmState.B;

					break;

				case FsmState.B:
					m_oCurChild = (m_oCurChild == null) ? oDL.FirstChild : m_oCurChild.NextSibling;
					m_nCurState = FsmState.C;

					break;

				case FsmState.C:
					if (m_oCurChild == null) {
						m_nCurState = FsmState.I;
						Success = true;
					}
					else {
						switch (m_oCurChild.Name.ToLower()) {
						case "#text":
							m_nCurState = FsmState.B;
							break;

						case "dt":
							m_nCurState = FsmState.D;
							break;

						default:
							m_nCurState = FsmState.H;
							break;
						} // switch
					} // if

					break;

				case FsmState.D:
					string sKey = m_oCurChild.InnerText;

					if (string.IsNullOrWhiteSpace(sKey)) {
						if (bFailOnEmptyDT)
							m_nCurState = FsmState.H;
						else if (m_sCurKey == null)
							m_nCurState = FsmState.H;
						else
							m_nCurState = FsmState.E;
					}
					else {
						m_sCurKey = sKey.Trim();
						m_nCurState = FsmState.E;
					} // if

					break;

				case FsmState.E:
					m_oCurChild = m_oCurChild.NextSibling;
					m_nCurState = FsmState.F;

					break;

				case FsmState.F:
					if (m_oCurChild == null)
						m_nCurState = FsmState.H;
					else {
						switch (m_oCurChild.Name.ToLower()) {
						case "#text":
							m_nCurState = FsmState.E;
							break;

						case "dd":
							m_nCurState = FsmState.G;
							break;

						default:
							m_nCurState = FsmState.H;
							break;
						} // switch
					} // if

					break;

				case FsmState.G:
					string sValue = m_oCurChild.InnerText.Trim();

					if (Data.ContainsKey(m_sCurKey)) {
						Data[m_sCurKey] += "\n" + sValue;
						Debug("Appended[{0}] = {1}", m_sCurKey, sValue);
					}
					else {
						Data[m_sCurKey] = sValue;
						Debug("New[{0}] = {1}", m_sCurKey, sValue);
					} // if

					m_nCurState = FsmState.B;

					break;

				case FsmState.H:
					Warn("Wrong DL structure encountered.");
					Success = false;
					m_nCurState = FsmState.I;

					break;
				} // switch current state
			} // while
		} // Parse

		private HtmlNode m_oCurChild;

		private enum FsmState {
			A, B, C, D, E, F, G, H, I
		} // FsmState

		private FsmState m_nCurState;

		private string m_sCurKey;

	} // class DLParser

} // namespace Ezbob.HmrcHarvester
