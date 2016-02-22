namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using JetBrains.Annotations;

	internal class GetCustomerCompanyID : AStoredProcedure {
		public GetCustomerCompanyID(AConnection db, ASafeLog log) : base(db, log) { } // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && (Now > longAgo);
		} // HasValidParameters

		[UsedImplicitly]
		public int CustomerID { get; set; }

		[UsedImplicitly]
		public DateTime Now { get; set; }

		[UsedImplicitly]
		[Direction(ParameterDirection.Output)]
		public int CompanyID { get; set; }

		[UsedImplicitly]
		[Direction(ParameterDirection.Output)]
		[Length(50)]
		public string TypeOfBusiness { get; set; }

		private static readonly DateTime longAgo = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
	} // class GetCustomerCompanyID
} // namespace
