using System.Collections.Generic;
using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class Field

	public class Field {
		#region public

		#region constructor

		public Field() {
		} // constructor

		#endregion constructor

		public string SourcePath { get; set; }
		public List<Target> Targets { get; set; }

		#region method Validate

		public void Validate(FieldGroup oGroup, ASafeLog log) {
			if (string.IsNullOrWhiteSpace(SourcePath))
				throw new OwnException("Field source path not specified.");

			Targets.ForEach(t => t.Validate(oGroup, log));
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Source path: {1}\n{0}Targets:\n", sLinePrefix, SourcePath);

			Targets.ForEach(t => t.Log(sb, sLinePrefix + "\t"));
		} // Log

		#endregion method Log

		#region method SetValue

		public void SetValue(string sValue) {
			foreach (Target oTarget in Targets)
				oTarget.SetValue(sValue);
		} // SetValue

		#endregion method SetValue

		#endregion public
	} // class Field

	#endregion class Field
} // namespace Ezbob.ExperianParser
