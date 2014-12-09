using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {

	public class Target {

		public Target() {
		} // constructor

		public string Name { get; set; }
		public int Position { get; set; }
		public string Prefix { get; set; }
		public string Suffix { get; set; }
		public Transformation Transformation { get; set; }

		public void Validate(FieldGroup oGroup, ASafeLog log) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new OwnException("Field target name not specified.");

			Prefix = Prefix ?? "";
			Suffix = Suffix ?? "";

			if (Transformation != null)
				Transformation.Validate(log);

			oGroup.AddOutputFieldBuilder(this);
		} // Validate

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Name: {1}\n{0}Position: {2}\n{0}Prefix: {3}\n{0}Suffix: {5}\n{0}Transformation: {4}\n",
				sLinePrefix, Name, Position, Prefix, Transformation == null ? "no" : "yes", Suffix
			);

			if (Transformation != null)
				Transformation.Log(sb, sLinePrefix + "\t");

			sb.Append("\n");
		} // Log

		public string this[string sValue] {
			get {
				string sResult = (Transformation == null) ? sValue : Transformation.Apply(sValue);

				if (!string.IsNullOrEmpty(Prefix))
					sResult = Prefix + sResult;

				if (!string.IsNullOrEmpty(Suffix))
					sResult = sResult + Suffix;

				return sResult;
			} // get
		} // indexer

		public void SetBuilder(OutputFieldBuilder oBuilder) {
			m_oBuilder = oBuilder;
		} // SetBuilder

		public void SetValue(string sValue) {
			if (m_oBuilder != null)
				m_oBuilder.Set(this, sValue);
		} // SetValue

		private OutputFieldBuilder m_oBuilder;
	} // class Target

} // namespace Ezbob.ExperianParser
