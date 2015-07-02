namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;

	internal class CompanyFilesMarketplaceModelBuilder : MarketplaceModelBuilder {
		public CompanyFilesMarketplaceModelBuilder(ISession session) : base(session) {
			this.companyFilesRepo = new CompanyFilesMetaDataRepository(session);
		} // constructor

		public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo) {
			return "#";
		} // GetUrl

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			List<DateTime> filesDate = this.companyFilesRepo.GetByCustomerId(mp.Customer.Id).Select(x => x.Created).ToList();

			if (filesDate.Any())
				return filesDate.Min();

			return null;
		} // GetSeniority

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			return null;
		} // GetLastTransaction

		public override void UpdateLastTransactionDate(MP_CustomerMarketPlace mp) {
			// Nothing to do here, base method should not be executed.
		} // UpdateLastTransactionDate

		protected override PaymentAccountsModel GetPaymentAccountModel(
			MP_CustomerMarketPlace mp,
			DateTime? history,
			List<IAnalysisDataParameterInfo> av
		) {
			return new PaymentAccountsModel(mp, history);
		} // GetPaymentAccountModel

		protected override void InitializeSpecificData(
			MP_CustomerMarketPlace mp,
			MarketPlaceModel model,
			DateTime? history
		) {
			var cfm = new CompanyFilesModel { Files = new List<CompanyFile>() };
			IEnumerable<MP_CompanyFilesMetaData> files = this.companyFilesRepo.GetByCustomerId(mp.Customer.Id);

			foreach (var file in files) {
				cfm.Files.Add(new CompanyFile {
					FileName = file.FileName,
					Id = file.Id,
					Uploaded = file.Created
				});
			} // for each

			model.CompanyFiles = cfm;
		} // InitializeSpecificData

		private readonly CompanyFilesMetaDataRepository companyFilesRepo;
	} // class CompanyFilesMarketplaceModelBuilder
} // namespace