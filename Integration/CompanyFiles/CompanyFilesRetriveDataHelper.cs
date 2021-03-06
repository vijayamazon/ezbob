﻿namespace CompanyFiles {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;

	public class CompanyFilesRetriveDataHelper : MarketplaceRetrieveDataHelperBase {
		public CompanyFilesRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		) : base(helper, marketplace) {
		} // constructor

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return null;
		} // RetrieveCustomerSecurityInfo

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			return new ElapsedTimeInfo();
		} // RetrieveAndAggregate
	} // class CompanyFilesRetriveDataHelper
} // namespace
