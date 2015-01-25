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
			string enteredFullName = null;
			string experianFullName = null;

			var businessNames = new List<BusinessRelevance>();

			DB.ForEachRowSafe(
				sr => {
					string rowType = sr["RowType"];

					switch (rowType) {
					case "CompanyName":
						experianCompanyName = sr["ExperianCompanyName"];
						enteredCompanyName = sr["EnteredCompanyName"];
						enteredFullName = sr["FullName"];
						break;

					case "HmrcBusinessName":
						BusinessRelevance br = sr.Fill<BusinessRelevance>();

						if (!br.BelongsToCustomer.HasValue) {
							br.Name = AutomationCalculator.Utils.AdjustCompanyName(br.Name);
							businessNames.Add(br);
						} // if

						break;

					case "ExperianName":
						experianFullName = UpdateHmrcBusinessRelevance.Concat(
							sr["ForeName"],
							sr["MiddleInitial"],
							sr["Surname"]
						);
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
				bn.SetOtherNameAndBelongs(companyName, experianFullName ?? enteredFullName);

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

		private static string Concat(params string[] args) {
			var lst = new List<string>();

			foreach (string s in args) {
				if (string.IsNullOrWhiteSpace(s))
					continue;

				lst.Add(s.Trim());
			} // for

			return lst.Count > 0 ? string.Join(" ", lst) : null;
		} // Concat

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class BusinessRelevance : IParametrisable {
			public bool? BelongsToCustomer { get; set; }

			public int BusinessID { get; set; }
			public string Name { get; set; }
			public string OtherName { get; set; }
			public string FullName { get; set; }

			public static Type[] StructToSave() {
				return new[] {
					typeof(int), // business id
					typeof(bool?), // belongs to customer
					typeof(string), // business name (from Business table)
					typeof(string), // company name (from Experian or entered by customer)
					typeof(string) // customer full name
				};
			} // StructToSave

			public void SetOtherNameAndBelongs(string companyName, string fullName) {
				OtherName = companyName;
				FullName = fullName;

				BelongsToCustomer = !string.IsNullOrWhiteSpace(companyName) && (Name ?? string.Empty).Equals(companyName);

				if (!BelongsToCustomer.Value)
					BelongsToCustomer = !string.IsNullOrWhiteSpace(fullName) && (Name ?? string.Empty).Equals(fullName);
			} // SetOtherNameAndBelongs

			public object[] ToParameter() {
				return new object[] {
					BusinessID, BelongsToCustomer, Name, OtherName, FullName
				};
			} // ToParameter
		} // class BusinessRelevance

		private readonly int customerID;
		private readonly DateTime now;
	} // class UpdateHmrcBusinessRelevance
} // namespace
