
namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;
	using Web.Areas.Customer.Models;

	class CompanyFilesMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		private readonly CompanyFilesMetaDataRepository _companyFiles;
		public CompanyFilesMarketplaceModelBuilder(ISession session)
			: base(session)
		{
			_companyFiles = new CompanyFilesMetaDataRepository(session);
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			var companyFiles = new PaymentAccountsModel
			{
				displayName = mp.DisplayName,
				TotalNetInPayments = 0,
				MonthInPayments = 0,
				TotalNetOutPayments = 0,
				TransactionsNumber = 0,
				id = mp.Id,
				Status = ""  ,
			};
			return companyFiles;
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			model.CompanyFiles = BuildCompanyFiles(mp);
		}

		private CompanyFilesModel BuildCompanyFiles(MP_CustomerMarketPlace mp)
		{
			var model = new CompanyFilesModel() { Files = new List<CompanyFile>()};
			var files = _companyFiles.GetByCustomerId(mp.Customer.Id);
			
			foreach (var file in files)
			{
				model.Files.Add(new CompanyFile()
					{
						FileName = file.FileName,
						Id =  file.Id,
						Uploaded = file.Created
					});
			}
			return model;
		}
	}
}