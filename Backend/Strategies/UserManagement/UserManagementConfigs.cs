namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;

	internal class UserManagementConfigs {

		public UserManagementConfigs(AConnection oDB, ASafeLog oLog) {
			Underwriters = new SortedSet<string>();

			var sp = new GetUserManagementConfiguration(oDB, oLog);
			sp.ForEachResult<GetUserManagementConfiguration.ResultRow>(FillProp);
		} // constructor

		public string LoginValidationStringForWeb { get; set; }

		public int NumOfInvalidPasswordAttempts { get; set; }

		public int InvalidPasswordAttemptsPeriodSeconds { get; set; }

		public int InvalidPasswordBlockSeconds { get; set; }

		public string PasswordValidity { get; set; }

		public string LoginValidity { get; set; }

		public override string ToString() {
			var os = new StringBuilder();

			this.Traverse((oInstance, oPropertyInfo) => {
				var oValue = oPropertyInfo.GetValue(oInstance);

				if (oValue == null)
					os.AppendFormat("\t{0}: -- NULL --.\n", oPropertyInfo.Name);
				else
					os.AppendFormat("\t{0}: '{1}'.\n", oPropertyInfo.Name, oValue.ToString());
			});

			return "(\n" + os.ToString() + ")";
		} // ToString

		public SortedSet<string> Underwriters { get; private set; }

		private ActionResult FillProp(GetUserManagementConfiguration.ResultRow oRow) {
			// ReSharper disable EmptyGeneralCatchClause
			try {
				if (oRow.Name == "__UnderwriterLogin__")
					Underwriters.Add(oRow.Value.Trim().ToLower(CultureInfo.InvariantCulture));
				else {
					var oPropInfo = GetType().GetProperty(oRow.Name);

					if (oPropInfo == null)
						return ActionResult.Continue;

					if (oPropInfo.PropertyType == typeof (int))
						oPropInfo.SetValue(this, int.Parse(oRow.Value));
					else
						oPropInfo.SetValue(this, oRow.Value);
				} // if
			}
			catch (Exception) {
				// silently ignore
			} // try
			// ReSharper restore EmptyGeneralCatchClause

			return ActionResult.Continue;
		} // FillProp

		private class GetUserManagementConfiguration : AStoredProcedure {
			public GetUserManagementConfiguration(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return true;
			} // HasValidParameters

			public class ResultRow : AResultRow {
				[UsedImplicitly]
				public string Name { get; set; }

				[UsedImplicitly]
				public string Value { get; set; }

			} // ResultRow

		} // GetUserManagementConfiguration

	} // class UserManagementConfigs
} // namespace Ezbob.Backend.Strategies.UserManagement
