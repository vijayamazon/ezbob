using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class Target

	class Target {
		#region public

		#region constructor

		public Target() {
		} // constructor

		#endregion constructor

		public string Name { get; set; }
		public int Position { get; set; }
		public string Prefix { get; set; }
		public Transformation Transformation { get; set; }

		#region method Validate

		public void Validate(FieldGroup oGroup, ASafeLog log) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new OwnException("Field target name not specified.");

			Prefix = Prefix ?? "";

			if (Transformation != null)
				Transformation.Validate(log);

			oGroup.AddOutputFieldBuilder(this);
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Name: {1}\n{0}Position: {2}\n{0}Prefix: {3}\n{0}Transformation: {4}\n",
				sLinePrefix, Name, Position, Prefix, Transformation == null ? "no" : "yes"
			);

			if (Transformation != null)
				Transformation.Log(sb, sLinePrefix + "\t");

			sb.Append("\n");
		} // Log

		#endregion method Log

		#region indexer

		public string this[string sValue] {
			get {
				string sResult = (Transformation == null) ? sValue : Transformation.Apply(sValue);

				if (!string.IsNullOrEmpty(Prefix))
					sResult = Prefix + sResult;

				return sResult;
			} // get
		} // indexer

		#endregion indexer

		#region method SetBuilder

		public void SetBuilder(OutputFieldBuilder oBuilder) {
			m_oBuilder = oBuilder;
		} // SetBuilder

		#endregion method SetBuilder

		#region method SetValue

		public void SetValue(string sValue) {
			if (m_oBuilder != null)
				m_oBuilder.Set(this, sValue);
		} // SetValue

		#endregion method SetValue

		#endregion public

		private OutputFieldBuilder m_oBuilder;
	} // class Target

	#endregion class Target
} // namespace Ezbob.ExperianParser
