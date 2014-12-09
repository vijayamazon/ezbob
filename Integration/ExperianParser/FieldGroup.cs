using System.Collections.Generic;
using System.Text;
using System.Xml;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {

	public class FieldGroup {

		public FieldGroup() {
			OutputFieldBuilders = new SortedDictionary<string, OutputFieldBuilder>();
		} // constructor

		public string Name { get; set; }
		public string PathToParent { get; set; }
		public List<Field> Fields { get; set; }
		public Dictionary<string, string> MetaData { get; set; }

		public List<FieldGroup> Children { get; set; }

		public SortedDictionary<string, OutputFieldBuilder> OutputFieldBuilders { get; private set; }

		public bool HasChildren() {
			return !ReferenceEquals(Children, null) && (Children.Count > 0);
		} // HasChildren

		public void Validate(ASafeLog log) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new OwnException("Field group name not specified.");

			OutputFieldBuilders.Clear();

			PathToParent = (PathToParent ?? "").Trim();

			Fields.ForEach(f => f.Validate(this, log));

			if (HasChildren())
				Children.ForEach(c => c.Validate(log));
		} // Validate

		public void Log(ASafeLog log) {
			if (log == null)
				return;

			var sb = new StringBuilder();

			sb.AppendFormat("\nGroup\n\tName: {0}\n\tPath to parent: {1}\n\tFields:\n", Name, PathToParent);

			Fields.ForEach(f => f.Log(sb, "\t\t"));

			log.Debug(sb.ToString());
		} // Log

		public void AddOutputFieldBuilder(Target oTarget) {
			if (OutputFieldBuilders.ContainsKey(oTarget.Name))
				OutputFieldBuilders[oTarget.Name].Add(oTarget);
			else
				OutputFieldBuilders[oTarget.Name] = new OutputFieldBuilder(oTarget);
		} // AddOutputFieldBuilder

	} // class FieldGroup

} // namespace Ezbob.ExperianParser
