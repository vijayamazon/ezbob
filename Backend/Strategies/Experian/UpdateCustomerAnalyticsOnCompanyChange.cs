namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class UpdateCustomerAnalyticsOnCompanyChange : AStrategy {
		public override string Name {
			get { return "UpdateCustomerAnalyticsOnCompanyChange"; }
		} // Name

		public UpdateCustomerAnalyticsOnCompanyChange(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override void Execute() {
			var company = new CompanyDetails(this.customerID);

			var businessData = ExperianCompanyCheck.GetBusinessData(
				company.IsLimited,
				company.ExperianRefNum,
				this.customerID,
				true,
				false
				);

			if (businessData == null) {
				DB.ExecuteNonQuery(
					"ClearCustomerAnalyticsCompany",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.customerID)
					);
			} else if (company.IsLimited) {
				ExperianCompanyCheck.UpdateAnalyticsForLimited(
					(LimitedResults)businessData,
					this.customerID
					);
			} else {
				ExperianCompanyCheck.UpdateAnalyticsForNonLimited(
					businessData.MaxBureauScore,
					company.ExperianRefNum,
					this.customerID
					);
			} // if
		} // Execute

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class CompanyDetails {
			public int CompanyID { get; set; }

			public string TypeOfBusinessStr {
				get { return TypeOfBusiness == null ? string.Empty : TypeOfBusiness.Value.ToString(); }
				set {
					TypeOfBusiness tob;

					if (Enum.TryParse(value, out tob))
						TypeOfBusiness = tob;
				} // set
			} // TypeOfBusinessStr

			public TypeOfBusiness? TypeOfBusiness { get; private set; }

			public bool IsLimited {
				get { return (TypeOfBusiness != null) && (TypeOfBusiness.Value.Reduce() == TypeOfBusinessReduced.Limited); }
			} // IsLimited

			public string ExperianRefNum { get; set; }

			public CompanyDetails(int customerID) {
				var sp = new LoadCustomerCompanyDetails(customerID, Library.Instance.DB, Library.Instance.Log);
				sp.FillFirst(this);
			} // constructor

			private class LoadCustomerCompanyDetails : AStoredProcedure {
				// ReSharper disable once MemberCanBePrivate.Local
				public int CustomerID { get; set; }

				public LoadCustomerCompanyDetails(int customerID, AConnection db, ASafeLog log)
					: base(db, log) {
					CustomerID = customerID;
				} // constructor

				public override bool HasValidParameters() {
					return CustomerID > 0;
				} // HasValidParameters
			} // class LoadCustomerCompanyDetails
		} // class CompanyDetails

		private readonly int customerID;
	} // class UpdateCustomerAnalyticsOnCompanyChange
} // namespace
