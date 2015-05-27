namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	internal class SpLoadDataForEsign : AStoredProc {

		public SpLoadDataForEsign(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			this.errorList = new List<string>();
		} // constructor

		public override bool HasValidParameters() {
			return
				(CustomerID > 0) &&
				(TemplateID > 0);
		} // HasValidParameters

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

		public bool IsReady { get; private set; }

		public Person Customer { get; private set; }

		public Company Company { get; private set; }

		public List<Person> Directors { get; private set; }

		public List<Person> ExperianDirectors { get; private set; }

		public Template Template { get; private set; }

		public IReadOnlyCollection<string> ErrorList { get { return this.errorList.AsReadOnly(); } }

		public void Load() {
			this.errorList.Clear();
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
					string msg = string.Format("Unexpected row type received from DB: '{0}'.", sRowType);
					Log.Warn(msg);
					this.errorList.Add(msg);
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

				default: {
					string msg = string.Format("Unexpected row type received from DB: '{0}'.", sRowType);
					Log.Warn(msg);
					this.errorList.Add(msg);
					}
					break;
				} // switch

				return ActionResult.Continue;
			});

			Directors = oDirs.Count > 0 ? oDirs.Values.ToList() : new List<Person>();

			ExperianDirectors = oExpDirs.Count > 0 ? oExpDirs.Values.ToList() : new List<Person>();

			FillTemplate();
		} // Load

		private enum RowType {
			Customer,
			Company,
			Director,
			ExperianDirector,
			Template,
		} // RowType

		private void FillTemplate() {
			IsReady = false;

			if (Company == null) {
				string msg = string.Format("No company found for customer {0}.", CustomerID);
				Log.Warn(msg);
				this.errorList.Add(msg);
				return;
			} // if

			if (Template == null) {
				string msg = string.Format("No template found for id {0}.", TemplateID);
				Log.Warn(msg);
				this.errorList.Add(msg);
				return;
			} // if

			if (Customer == null) {
				string msg = string.Format("No customer found for id {0}.", CustomerID);
				Log.Warn(msg);
				this.errorList.Add(msg);
				return;
			} // if

			if (Template.IsOfKnownType)
				Template.FillCommonDetails(Customer, Company);

			IsReady = true;
		} // FillTemplate

		private readonly List<string> errorList;
	} // class SpLoadDataForEsign
} // namespace
