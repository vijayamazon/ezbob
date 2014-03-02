namespace LandRegistryLib
{
	using System.Collections.Generic;
	using LREnquiryServiceNS;
	using LRResServiceNS;
	using Q1AddressType = LRResServiceNS.Q1AddressType;

	public class LandRegistryModelBuilder
	{

		public LandRegistryResponseType GetResponseType(int value)
		{
			LandRegistryResponseType type;
			switch (value)
			{
				case 1:
				case 10:
					type = LandRegistryResponseType.Acknowledgement;
					break;
				case 2:
				case 20:
					type = LandRegistryResponseType.Rejection;
					break;
				case 3:
				case 30:
					type = LandRegistryResponseType.Success;
					break;
				default:
					type = LandRegistryResponseType.Unkown;
					break;
			}
			return type;
		}




		public LandRegistryEnquiryModel BuildEnquiryModel(string responseXml)
		{
			var response = XmlHelper.DeserializeObject<ResponseSearchByPropertyDescriptionV2_0Type>(responseXml); 
			//XmlHelper.XmlDeserializeFromString<ResponseSearchByPropertyDescriptionV2_0Type>(responseXml);
			return BuildEnquiryModel(response);
		}

		public LandRegistryEnquiryModel BuildEnquiryModel(ResponseSearchByPropertyDescriptionV2_0Type response)
		{
			var model = new LandRegistryEnquiryModel { Titles = new List<LandRegistryEnquiryTitle>() };
			var data = response.GatewayResponse.Results;
			foreach (var title in data.Title)
			{
				var lrTitle = new LandRegistryEnquiryTitle
				{
					TitleNumber = title.TitleNumber == null ? null : title.TitleNumber.Value,
					Postcode = (title.Address.PostcodeZone == null || title.Address.PostcodeZone.Postcode == null) ? null : title.Address.PostcodeZone.Postcode.Value,
					BuildingName = title.Address.BuildingName == null ? null : title.Address.BuildingName.Value,
					CityName = title.Address.CityName== null ? null : title.Address.CityName.Value,
					BuildingNumber = title.Address.BuildingNumber== null ? null : title.Address.BuildingNumber.Value,
					StreetName = title.Address.StreetName== null ? null : title.Address.StreetName.Value,
					SubBuildingName = title.Address.SubBuildingName== null ? null : title.Address.SubBuildingName.Value
				};
				model.Titles.Add(lrTitle);
			}
			return model;
		}

		public LandRegistryResModel BuildResModel(string responseXml)
		{
			var response = XmlHelper.XmlDeserializeFromString<ResponseOCWithSummaryV2_1Type>(responseXml);
			return BuildResModel(response);
		}

		public LandRegistryResModel BuildResModel(ResponseOCWithSummaryV2_1Type response)
		{
			var model = new LandRegistryResModel();
			var data = response.GatewayResponse.Results.OCSummaryData;
			model.TitleNumber = data.Title.TitleNumber.Value;
			model.Indicators = new List<string>();
			if (data.Title.CommonholdIndicator.Value == false)
			{
				model.Indicators.Add(LandRegistryIndicatorText.CommonholdIndicator);
			}

			if (data.PricePaidEntry != null)
			{
				model.PricePaidInfills = GetInfills(data.PricePaidEntry.EntryDetails.Infills);
			}

			model.PropertyAddresses = GetAddresses(data.PropertyAddress);

			model.Proprietorship = new LandRegistryProprietorshipModel
				{
					CurrentProprietorshipDate = data.Proprietorship.CurrentProprietorshipDate.Value,
					ProprietorshipParties = new List<ProprietorshipPartyModel>()
				};

			for (int i = 0; i < data.Proprietorship.Items.Length; ++i)
			{
				var proprietorship = data.Proprietorship.Items[i];
				ItemsChoiceType type = data.Proprietorship.ItemsElementName[i];
				var lrProprietorship = new ProprietorshipPartyModel();
				if (type == ItemsChoiceType.CautionerParty)
				{
					lrProprietorship.ProprietorshipType = "Cautioner Party";
				}
				else if (type == ItemsChoiceType.RegisteredProprietorParty)
				{
					lrProprietorship.ProprietorshipType = "Registered Proprietorship Party";
				}
				if (proprietorship.Item.GetType() == typeof(Q1PrivateIndividualType))
				{
					lrProprietorship.ProprietorshipPartyType = "Private Individual";
					lrProprietorship.PrivateIndividualForename = ((Q1PrivateIndividualType)proprietorship.Item).Name.ForenamesName.Value;
					lrProprietorship.PrivateIndividualSurname = ((Q1PrivateIndividualType)proprietorship.Item).Name.SurnameName.Value;
				}
				else if (proprietorship.Item.GetType() == typeof(Q1OrganizationType))
				{
					lrProprietorship.ProprietorshipPartyType = "Organization";
					lrProprietorship.CompanyRegistrationNumber = ((Q1OrganizationType)proprietorship.Item).CompanyRegistrationNumber.Value;
					lrProprietorship.CompanyName = ((Q1OrganizationType)proprietorship.Item).Name.Value;
				}
				lrProprietorship.ProprietorshipAddresses = GetAddresses(proprietorship.Address);

				model.Proprietorship.ProprietorshipParties.Add(lrProprietorship);
			}

			model.Restrictions = new List<LandRegistryRestrictionModel>();

			foreach (var restriction in data.RestrictionDetails)
			{
				var lrRestriction = new LandRegistryRestrictionModel();
				switch (restriction.ItemElementName)
				{
					case ItemChoiceType.ChargeRestriction:
						lrRestriction.Type = "Charge Restriction";
						break;
					case ItemChoiceType.ChargeRelatedRestriction:
						lrRestriction.Type = "Charge Related Restriction";
						break;

					case ItemChoiceType.NonChargeRestriction:
						lrRestriction.Type = "Non Charge Restriction";
						break;
				}

				lrRestriction.TypeCode = (RestrictionTypeCode)(int)restriction.Item.RestrictionTypeCode.Value;
				lrRestriction.EntryText = restriction.Item.EntryDetails.EntryText.Value;
				lrRestriction.EntryNumber = restriction.Item.EntryDetails.EntryNumber.Value;
				if (restriction.Item.EntryDetails.Item.GetType() == typeof(ScheduleCodeType))
				{
					var code = (RestictionScheduleCode)(int)((ScheduleCodeType)restriction.Item.EntryDetails.Item);
					lrRestriction.ScheduleCode = code.DescriptionAttr();
				}
				else if (restriction.Item.EntryDetails.Item.GetType() == typeof(SubRegisterCodeType))
				{
					var code = (RestrictionSubRegisterCode)(int)((SubRegisterCodeType)restriction.Item.EntryDetails.Item);
					lrRestriction.SubRegisterCode = code.DescriptionAttr();
				}

				lrRestriction.Infills = GetInfills(restriction.Item.EntryDetails.Infills);

				model.Restrictions.Add(lrRestriction);
			}

			model.Indicators.AddRange(GetIndicators(data.RegisterEntryIndicators));

			return model;
		}

		private List<LandRegistryAddressModel> GetAddresses(IEnumerable<Q1AddressType> propertyAddress)
		{
			var addresses = new List<LandRegistryAddressModel>();
			foreach (var address in propertyAddress)
			{
				var lrAddress = new LandRegistryAddressModel
					{
						PostCode =
							address.PostcodeZone != null && address.PostcodeZone.Postcode != null ? address.PostcodeZone.Postcode.Value : null
					};
				if (address.AddressLine != null && address.AddressLine.Line != null)
				{
					foreach (var line in address.AddressLine.Line)
					{
						lrAddress.Lines += " " + line.Value;
					}
					addresses.Add(lrAddress);
				}
			}
			return addresses;
		}

		private List<KeyValuePair<string, string>> GetInfills(IEnumerable<object> infills)
		{
			var lrInfills = new List<KeyValuePair<string, string>>();
			
			if (infills == null) return lrInfills;

			foreach (var infill in infills)
			{

				switch (infill.GetType().Name)
				{
					case "AmountInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "Amount", Value = ((AmountInfillType)infill).Value });
						break;
					case "ChargeDateInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "ChargeDate", Value = ((ChargeDateInfillType)infill).Value.ToString("dd/MM/yyyy") });
						break;
					case "ChargePartyInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "ChargeParty", Value = ((ChargePartyInfillType)infill).Value });
						break;
					case "DateInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "Date", Value = ((DateInfillType)infill).Value.ToString("dd/MM/yyyy") });
						break;
					case "DeedDateInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "DeedDate", Value = ((DeedDateInfillType)infill).Value });
						break;
					case "DeedExtentInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "DeedExtent", Value = ((DeedExtentInfillType)infill).Value });
						break;
					case "DeedPartyInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "DeedParty", Value = ((DeedPartyInfillType)infill).Value });
						break;
					case "DeedTypeInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "DeedType", Value = ((DeedTypeInfillType)infill).Value });
						break;
					case "ExtentOfLandInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "ExtentOfLand", Value = ((ExtentOfLandInfillType)infill).Value });
						break;
					case "MiscTextInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "MiscellaneousText", Value = ((MiscTextInfillType)infill).Value });
						break;
					case "NameInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "Name", Value = ((NameInfillType)infill).Value });
						break;
					case "NoteInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "Note", Value = ((NoteInfillType)infill).Value });
						break;
					case "OptMiscTextInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "OptionalMiscText", Value = ((OptMiscTextInfillType)infill).Value });
						break;
					case "PlansRefInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "PlansReference", Value = ((PlansRefInfillType)infill).Value });
						break;
					case "TitleNumberInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "TitleNumber", Value = ((TitleNumberInfillType)infill).Value });
						break;
					case "VerbatimTextInfillType":
						lrInfills.Add(new KeyValuePair<string, string> { Key = "VerbatimText", Value = ((VerbatimTextInfillType)infill).Value });
						break;
				}
			}

			return lrInfills;
		}

		private IEnumerable<string> GetIndicators(Q1RegisterEntryIndicatorsType indicators)
		{
			var lrIndicators = new List<string>();

			if (indicators.AgreedNoticeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.AgreedNoticeIndicator); }
			if (indicators.BankruptcyIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.BankruptcyIndicator); }
			if (indicators.CautionIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.CautionIndicator); }
			if (indicators.CCBIIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.CCBIIndicator); }
			if (indicators.ChargeeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.ChargeeIndicator); }
			if (indicators.ChargeRelatedRestrictionIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.ChargeRelatedRestrictionIndicator); }
			if (indicators.ChargeRestrictionIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.ChargeRestrictionIndicator); }
			if (indicators.CreditorsNoticeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.CreditorsNoticeIndicator); }
			if (indicators.DeathOfProprietorIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.DeathOfProprietorIndicator); }
			if (indicators.DeedOfPostponementIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.DeedOfPostponementIndicator); }
			if (indicators.DiscountChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.DiscountChargeIndicator); }
			if (indicators.EquitableChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.EquitableChargeIndicator); }
			if (indicators.GreenOutEntryIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.GreenOutEntryIndicator); }
			if (indicators.HomeRightsChangeOfAddressIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.HomeRightsChangeOfAddressIndicator); }
			if (indicators.HomeRightsIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.HomeRightsIndicator); }
			if (indicators.LeaseHoldTitleIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.LeaseHoldTitleIndicator); }
			if (indicators.MultipleChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.MultipleChargeIndicator); }
			if (indicators.NonChargeRestrictionIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.NonChargeRestrictionIndicator); }
			if (indicators.NotedChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.NotedChargeIndicator); }
			if (indicators.PricePaidIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.PricePaidIndicator); }
			if (indicators.PropertyDescriptionNotesIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.PropertyDescriptionNotesIndicator); }
			if (indicators.RentChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.RentChargeIndicator); }
			if (indicators.RightOfPreEmptionIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.RightOfPreEmptionIndicator); }
			if (indicators.ScheduleOfLeasesIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.ScheduleOfLeasesIndicator); }
			if (indicators.SubChargeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.SubChargeIndicator); }
			if (indicators.UnidentifiedEntryIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.UnidentifiedEntryIndicator); }
			if (indicators.UnilateralNoticeBeneficiaryIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.UnilateralNoticeBeneficiaryIndicator); }
			if (indicators.UnilateralNoticeIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.UnilateralNoticeIndicator); }
			if (indicators.VendorsLienIndicator.Value) { lrIndicators.Add(LandRegistryIndicatorText.VendorsLienIndicator); }
			return lrIndicators;
		}

		//public LandRegistryAcknowledgementModel BuildAcknowledgmentModel()
		//{
			
		//}

		//public LandRegistryRejectionModel BuildRejectionModel()
		//{

		//}
	}
}
