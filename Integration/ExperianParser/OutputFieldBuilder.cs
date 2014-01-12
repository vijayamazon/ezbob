using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.ExperianParser {
	#region class OutputFieldBuilder

	public class OutputFieldBuilder {
		#region public

		#region constructor

		public OutputFieldBuilder(Target oTarget) {
			m_oValues = new SortedDictionary<int, string>();
			m_oTargets = new SortedDictionary<int, Target>();
			m_oTargets[oTarget.Position] = oTarget;
			oTarget.SetBuilder(this);
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(Target oTarget) {
			if (m_oTargets.ContainsKey(oTarget.Position)) {
				throw new OwnException(
					"Duplicate target field configured; target field name {0}, position {1}",
					oTarget.Name, oTarget.Position
				);
			} // if

			m_oTargets[oTarget.Position] = oTarget;
			oTarget.SetBuilder(this);
		} // Add

		#endregion method Add

		#region method Clear

		public void Clear() {
			m_oValues.Clear();
		} // Clear

		#endregion method Clear

		#region method Set

		public void Set(Target oTarget, string sValue) {
			m_oValues[oTarget.Position] = sValue;
		} // Set

		#endregion method Set

		#region method Build

		public string Build() {
			var sb = new StringBuilder();

			foreach (KeyValuePair<int, Target> pair in m_oTargets) {
				int nIdx = pair.Key;
				Target oTarget = pair.Value;

				string sValue = null;

				if (m_oValues.ContainsKey(nIdx))
					sValue = m_oValues[nIdx];

				sb.Append(oTarget[sValue]);
			} // for each target

			return sb.ToString();
		} // Build

		#endregion method Build

		#endregion public

		#region private

		private readonly SortedDictionary<int, Target> m_oTargets;
		private readonly SortedDictionary<int, string> m_oValues;

		#endregion private
	} // class OutputFieldBuilder

	#endregion class OutputFieldBuilder
} // namespace Ezbob.ExperianParser
