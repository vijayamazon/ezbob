namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class UpdateHmrcBusinessRelevance : AStrategy {
		public override string Name {
			get { return "UpdateHmrcBusinessRelevance"; }
		} // Name

		public UpdateHmrcBusinessRelevance(int customerID, DateTime? now = null) {
			this.customerID = customerID;
			this.now = now ?? DateTime.UtcNow;
		} // constructor

		public override void Execute() {
			string experianCompanyName = null;
			string enteredCompanyName = null;

			var businessNames = new List<BusinessRelevance>();

			DB.ForEachRowSafe(
				sr => {
					string rowType = sr["RowType"];

					switch (rowType) {
					case "CompanyName":
						experianCompanyName = sr["ExperianCompanyName"];
						enteredCompanyName = sr["EnteredCompanyName"];
						break;

					case "HmrcBusinessName":
						BusinessRelevance br = sr.Fill<BusinessRelevance>();

						if (!br.BelongsToCustomer.HasValue) {
							br.Name = AutomationCalculator.Utils.AdjustCompanyName(br.Name);
							businessNames.Add(br);
						} // if

						break;
					} // switch
				},
				"LoadCustomerCompanyNames",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("Now", this.now)
				);

			string companyName = AutomationCalculator.Utils.AdjustCompanyName(experianCompanyName);
			if (companyName == string.Empty)
				companyName = AutomationCalculator.Utils.AdjustCompanyName(enteredCompanyName);

			if ((companyName == string.Empty) || (businessNames.Count < 1))
				return;

			foreach (BusinessRelevance bn in businessNames)
				bn.SetBelongs(companyName);

			DB.ExecuteNonQuery(
				"UpdateHmrcBusinessRelevance",
				CommandSpecies.StoredProcedure,
				DB.CreateTableParameter(
					typeof(BusinessRelevance),
					"RelevanceList",
					businessNames,
					o => ((BusinessRelevance)o).ToParameter(),
					BusinessRelevance.StructToSave()
					)
				);
		} // Execute

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class BusinessRelevance : IParametrisable {
			public bool? BelongsToCustomer { get; set; }

			public int BusinessID { get; set; }
			public string Name { get; set; }

			public static Type[] StructToSave() {
				return new[] {
					typeof(int), typeof(bool?),
				};
			} // StructToSave

			public void SetBelongs(string companyName) {
				BelongsToCustomer = !string.IsNullOrWhiteSpace(companyName) && (Name ?? string.Empty).Equals(companyName);
			} // SetBelongs

			public object[] ToParameter() {
				return new object[] {
					BusinessID, BelongsToCustomer,
				};
			} // ToParameter
		} // class BusinessRelevance

		private readonly int customerID;
		private readonly DateTime now;
	} // class UpdateHmrcBusinessRelevance
} // namespace
