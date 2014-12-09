using System.Collections.Generic;
using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {

	public class Field {

		public Field() {
		} // constructor

		public string SourcePath { get; set; }
		public List<Target> Targets { get; set; }

		public void Validate(FieldGroup oGroup, ASafeLog log) {
			if (string.IsNullOrWhiteSpace(SourcePath))
				throw new OwnException("Field source path not specified.");

			Targets.ForEach(t => t.Validate(oGroup, log));
		} // Validate

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Source path: {1}\n{0}Targets:\n", sLinePrefix, SourcePath);

			Targets.ForEach(t => t.Log(sb, sLinePrefix + "\t"));
		} // Log

		public void SetValue(string sValue) {
			foreach (Target oTarget in Targets)
				oTarget.SetValue(sValue);
		} // SetValue

	} // class Field

} // namespace Ezbob.ExperianParser
