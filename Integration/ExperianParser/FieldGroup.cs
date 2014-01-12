using System.Collections.Generic;
using System.Text;
using System.Xml;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class FieldGroup

	public class FieldGroup {
		#region public

		#region constructor

		public FieldGroup() {
			OutputFieldBuilders = new SortedDictionary<string, OutputFieldBuilder>();
		} // constructor

		#endregion constructor

		#region configuration

		public string Name { get; set; }
		public string PathToParent { get; set; }
		public List<Field> Fields { get; set; }
		public Dictionary<string, string> MetaData { get; set; }

		public List<FieldGroup> Children { get; set; }

		public SortedDictionary<string, OutputFieldBuilder> OutputFieldBuilders { get; private set; }

		#endregion configuration

		#region method HasChildren

		public bool HasChildren() {
			return !ReferenceEquals(Children, null) && (Children.Count > 0);
		} // HasChildren

		#endregion method HasChildren

		#region method Validate

		public void Validate(ASafeLog log) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new OwnException("Field group name not specified.");

			OutputFieldBuilders.Clear();

			PathToParent = (PathToParent ?? "").Trim();

			Fields.ForEach(f => f.Validate(this, log));

			if (HasChildren())
				Children.ForEach(c => c.Validate(log));
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(ASafeLog log) {
			if (log == null)
				return;

			var sb = new StringBuilder();

			sb.AppendFormat("\nGroup\n\tName: {0}\n\tPath to parent: {1}\n\tFields:\n", Name, PathToParent);

			Fields.ForEach(f => f.Log(sb, "\t\t"));

			log.Debug(sb.ToString());
		} // Log

		#endregion method Log

		#region method AddOutputFieldBuilder

		public void AddOutputFieldBuilder(Target oTarget) {
			if (OutputFieldBuilders.ContainsKey(oTarget.Name))
				OutputFieldBuilders[oTarget.Name].Add(oTarget);
			else
				OutputFieldBuilders[oTarget.Name] = new OutputFieldBuilder(oTarget);
		} // AddOutputFieldBuilder

		#endregion method AddOutputFieldBuilder

		#endregion public
	} // class FieldGroup

	#endregion class FieldGroup
} // namespace Ezbob.ExperianParser
