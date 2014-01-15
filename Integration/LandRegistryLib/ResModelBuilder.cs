namespace LandRegistryLib
{
	using LandRegistryLib.LRRESServiceTestNS;
	using System;
	using System.Collections.Generic;

	public class ResModelBuilder
	{
		public LandRegistryResModel BuildResModel(ResponseOCWithSummaryV2_1Type response)
		{
			var model = new LandRegistryResModel();
			model.ActualPrice = response.GatewayResponse.Results.ActualPrice.GrossPriceAmount.Value;
			model.OfficialCopyDateTime = response.GatewayResponse.Results.OCSummaryData.OfficialCopyDateTime.Value;
			model.RegisteredProprietorParties = new List<RegisteredProprietorParty>();
			foreach (var item in response.GatewayResponse.Results.OCSummaryData.Proprietorship.Items)
			{
				var p = new RegisteredProprietorParty();
				if (item.Item.GetType() == typeof(Q1PrivateIndividualType))
				{
					p.ProprietorName = ((Q1PrivateIndividualType)item.Item).Name.ForenamesName.Value + " " +
											  ((Q1PrivateIndividualType)item.Item).Name.SurnameName.Value;
				}

				if (item.Item.GetType() == typeof(Q1OrganizationType))
				{
					p.ProprietorName = ((Q1OrganizationType)item.Item).Name.Value;
				}

				model.RegisteredProprietorParties.Add(p);

			}
			model.Restrictions = new List<Restriction>();
			foreach (var item in response.GatewayResponse.Results.OCSummaryData.RestrictionDetails)
			{
				var r = new Restriction
				{
					Description = item.Item.EntryDetails.EntryText.Value,
					Type = item.ItemElementName.ToString(),
				};
				/*
				 * 
				 */
				model.Restrictions.Add(r);
			}
			model.Charges = new List<Charge>();
			foreach (var item in response.GatewayResponse.Results.OCSummaryData.Charge)
			{
				var c = new Charge
				{
					ChargeDate = item.ChargeDate.Value,
					Description = item.RegisteredCharge.EntryDetails.EntryText.Value,

					ChargeProprietorDate = item.ChargeProprietor.EntryDetails.RegistrationDate.Value,
					ChargeProprietorDescription = item.ChargeProprietor.EntryDetails.EntryText.Value
				};

				if (item.ChargeProprietor.ChargeeParty[0].Item.GetType() == typeof(Q1PrivateIndividualType))
				{
					var name = ((Q1PrivateIndividualType) item.ChargeProprietor.ChargeeParty[0].Item).Name;
					if (name != null)
					{
						c.ChargeProprietorName = 
							(name.ForenamesName != null ? name.ForenamesName.Value + " " : "") +
							(name.SurnameName != null ? name.SurnameName.Value : "");
					}
				}
				if (item.ChargeProprietor.ChargeeParty[0].Item.GetType() == typeof(Q1OrganizationType))
				{
					c.ChargeProprietorName = ((Q1OrganizationType)item.ChargeProprietor.ChargeeParty[0].Item).Name.Value;
				}
				model.Charges.Add(c);
			}

			return model;
		}
	}
}
