namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;

	public interface IMarketplaceModelBuilder {
		string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo);
		MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history);
		MarketPlaceDataModel CreateLightModel(MP_CustomerMarketPlace mp, DateTime? history);
		void UpdateOriginationDate(MP_CustomerMarketPlace mp);
		void SetAggregationData(MarketPlaceModel model, List<IAnalysisDataParameterInfo> av);
	}
}