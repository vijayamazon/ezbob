using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Integration.ChannelGrabberConfig {
	#region class SecurityData

	public class SecurityData : ICloneable {
		#region public

		#region constructor

		public SecurityData() {
			Fields = new List<FieldInfo>();
			ToStringArguments = new List<string>();
		} // SecurityData

		#endregion constructor

		#region properties

		public List<FieldInfo> Fields { get; set; }
		public List<string> ToStringArguments { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			Fields = Fields ?? new List<FieldInfo>();

			if (Fields.Count == 0)
				throw new ConfigException("Fields not specified.");

			var oUniquePositions = new HashSet<int>();

			Fields.ForEach(f => {
				f.Validate();

				if (f.UniqueIDPosition >= 0) {
					if (oUniquePositions.Contains(f.UniqueIDPosition))
						throw new ConfigException(string.Format("Too many fields are specified for position {0} in unique id.", f.UniqueIDPosition));

					oUniquePositions.Add(f.UniqueIDPosition);
				} // if
			});

			if (oUniquePositions.Count == 0)
				throw new ConfigException("No unique fields specified.");

			ToStringArguments = ToStringArguments ?? new List<string>();

			if (ToStringArguments.Count < 2)
				throw new ConfigException("ToString arguments not specified.");

			if (ToStringArguments.Any(string.IsNullOrEmpty))
				throw new ConfigException("Invalid arguments in ToString list.");

			for (int i = 1; i < ToStringArguments.Count; i++)
				if (typeof(AccountData).GetProperty(ToStringArguments[i]) == null)
					throw new ConfigException("Unknown property in ToString arguments: " + ToStringArguments[i]);
		} // Validate

		#endregion method Validate

		#region method AccountDataToString

		public string AccountDataToString(AccountData oAccountData) {
			var oValues = new List<object>();

			for (int i = 1; i < ToStringArguments.Count; i++) {
				PropertyInfo prop = typeof (AccountData).GetProperty(ToStringArguments[i]);
				oValues.Add(prop.GetGetMethod().Invoke(oAccountData, null));
			} // for

			return string.Format(ToStringArguments[0], oValues.ToArray());
		} // AccountDataToString

		#endregion method AccountDataToString

		#region method ToString

		public override string ToString() {
			var fld = new List<string>();

			Fields.ForEach(f => fld.Add(f.ToString()));

			return string.Format("fields:\n\t{0}\nToString({1})",
				string.Join("\n", fld.ToArray()),
				string.Join(", ", ToStringArguments)
			);
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			var oRes = new SecurityData {
				Fields = new List<FieldInfo>(),
				ToStringArguments = new List<string>()
			};

			foreach (FieldInfo fi in Fields)
				oRes.Fields.Add((FieldInfo)fi.Clone());

			foreach (string s in ToStringArguments)
				oRes.ToStringArguments.Add((string)s.Clone());

			return oRes;
		} // Clone

		#endregion method Clone

		#endregion public
	} // class SecurityData

	#endregion class SecurityData
} // namespace Integration.ChannelGrabberConfig
