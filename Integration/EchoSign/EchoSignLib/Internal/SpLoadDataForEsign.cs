namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	internal class SpLoadDataForEsign : AStoredProc {
		#region constructor

		public SpLoadDataForEsign(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region method HasValidParameters

		public override bool HasValidParameters() {
			return
				(CustomerID > 0) &&
				(TemplateID > 0);
		} // HasValidParameters

		#endregion method HasValidParameters

		#region DB arguments

		[UsedImplicitly]
		public int CustomerID { get; set; }

		[UsedImplicitly]
		public int TemplateID { get; set; }

		[UsedImplicitly]
		[FieldName("Directors")]
		public List<int> DirectorIDs { get; set; }

		[UsedImplicitly]
		[FieldName("ExperianDirectors")]
		public List<int> ExperianDirectorIDs { get; set; }

		#endregion DB arguments

		#region Loaded data

		public bool IsReady { get; private set; }

		public Person Customer { get; private set; }

		public Company Company { get; private set; }

		public List<Person> Directors { get; private set; }

		public List<Person> ExperianDirectors { get; private set; }

		public Template Template { get; private set; }

		#endregion Loaded data

		#region method Load

		public void Load() {
			IsReady = false;

			Template = null;
			Customer = null;
			Company = null;

			var oDirs = new SortedDictionary<int, Person>();

			var oExpDirs = new SortedDictionary<int, Person>();

			ForEachRowSafe((sr, bRowsetStart) => {
				RowType nRowType;

				string sRowType = sr["RowType"];

				if (!Enum.TryParse(sRowType, true, out nRowType)) {
					Log.Warn("Unexpected row type received from DB: '{0}'.", sRowType);
					return ActionResult.Continue;
				} // if

				switch (nRowType) {
				case RowType.Customer:
					Customer = sr.Fill<Person>();
					break;

				case RowType.Company:
					Company = sr.Fill<Company>();
					break;

				case RowType.Director: {
					Person p = sr.Fill<Person>();
					oDirs[p.ID] = p;
				}
					break;

				case RowType.ExperianDirector: {
					Person p = sr.Fill<Person>();
					oExpDirs[p.ID] = p;
				}
					break;

				case RowType.Template:
					Template = sr.Fill<Template>();
					break;

				default:
					Log.Warn("Unexpected row type received from DB: '{0}'.", sRowType);
					break;
				} // switch

				return ActionResult.Continue;
			});

			Directors = oDirs.Count > 0 ? oDirs.Values.ToList() : new List<Person>();

			ExperianDirectors = oExpDirs.Count > 0 ? oExpDirs.Values.ToList() : new List<Person>();

			FillTemplate();
		} // Load

		#endregion method Load

		#region private

		#region enum RowType

		private enum RowType {
			Customer,
			Company,
			Director,
			ExperianDirector,
			Template,
		} // RowType

		#endregion enum RowType

		#region method FillTemplate

		private void FillTemplate() {
			IsReady = false;

			if (Company == null) {
				Log.Warn("No company found for customer {0}.", CustomerID);
				return;
			} // if

			if (Template == null) {
				Log.Warn("No template found for id {0}.", TemplateID);
				return;
			} // if

			if (Customer == null) {
				Log.Warn("No customer found for id {0}.", CustomerID);
				return;
			} // if

			if (Template.IsOfKnownType)
				Template.FillCommonDetails(Customer, Company);

			IsReady = true;
		} // FillTemplate

		#endregion method FillTemplate

		#endregion private
	} // class SpLoadDataForEsign
} // namespace
