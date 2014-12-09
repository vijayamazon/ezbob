using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {

	public class FieldInfo : ICloneable {

		public FieldInfo() {
			PropertyName = "";
			NodeName = "";
			UniqueIDPosition = -1;
			Type = "";
			Caption = "";
			Default = "";
			ValidationRules = new ValidationRules();
			ValidationMessages = new List<ValidationMessage>();
		} // constructor

		public string PropertyName { get; set; }
		public string NodeName { get; set; }
		public int UniqueIDPosition { get; set; }
		public string Type { get; set; }
		public string Caption { get; set; }
		public string Default { get; set; }
		public ValidationRules ValidationRules { get; set; }
		public List<ValidationMessage> ValidationMessages { get; set; }

		public void Validate() {
			PropertyName = (PropertyName ?? "").Trim();

			if (typeof(AccountData).GetProperty(PropertyName) == null)
				throw new ConfigException("Unknown property: " + PropertyName);

			NodeName = (NodeName ?? "").Trim();
			Type = (Type ?? "").Trim();
			Caption = (Caption ?? "").Trim();
			Default = (Default ?? "").Trim();

			ValidationRules = ValidationRules ?? new ValidationRules();
			ValidationMessages = ValidationMessages ?? new List<ValidationMessage>();

			if (NodeName == "") {
				if ((Type == "") || (Caption == ""))
					throw new ConfigException("NodeName is not specified.");
			} // if

			if ((Type == "") && (Caption != ""))
				throw new ConfigException("Caption is not specified.");

			if ((Type != "") && (Caption == ""))
				throw new ConfigException("Type is not specified.");

			if ((Type == "") && (Caption == "")) {
				if (NodeName == "")
					throw new ConfigException("Type and Caption are not specified.");
			} // if

			ValidationRules.Validate();

			ValidationMessages.ForEach( vm => vm.Validate() );
		} // Validate

		public override string ToString() {
			return string.Format(
				"{0} <-> {1} = {2}\n\t\tDisplay as {4} with caption{5}\n\t\tDefault: {3}",
				PropertyName,
				NodeName,
				UniqueIDPosition >= 0 ? "unique key position " + UniqueIDPosition.ToString() : "not unique",
				Default ?? "--null--",
				Type,
				Caption
			);
		} // ToString

		public object Clone() {
			var fi = new FieldInfo {
				PropertyName = (string)this.PropertyName.Clone(),
				NodeName = (string)(this.NodeName ?? "").Clone(),
				UniqueIDPosition = this.UniqueIDPosition,
				Type = (string)(this.Type ?? "").Clone(),
				Caption = (string)(this.Caption ?? "").Clone(),
				Default = (string)(this.Default ?? "").Clone(),
				ValidationRules = (ValidationRules)this.ValidationRules.Clone(),
				ValidationMessages = new List<ValidationMessage>()
			};

			foreach (ValidationMessage msg in ValidationMessages)
				fi.ValidationMessages.Add((ValidationMessage)msg.Clone());

			return fi;
		} // Clone

	} // class FieldInfo

} // namespace Integration.ChannelGrabberConfig
