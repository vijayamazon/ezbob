namespace LandRegistryLib
{
	using System.Collections.Generic;
	using LRResServiceNS;

	public class ResModelBuilder
	{
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
				foreach (var infill in data.PricePaidEntry.EntryDetails.Infills)
				{
					if (infill.GetType() == typeof(AmountInfillType))
					{
						model.PricePaidEntryAmount = ((AmountInfillType)infill).Value;
					}

					if (infill.GetType() == typeof(DateInfillType))
					{
						model.PricePaidEntryDate = ((DateInfillType)infill).Value;
					}
				}
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

				lrRestriction.Infills = new List<KeyValuePair<string, string>>();

				foreach (var infill in restriction.Item.EntryDetails.Infills)
				{

					switch (infill.GetType().Name)
					{
						case "AmountInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "Amount", Value = ((AmountInfillType)infill).Value });
							break;
						case "ChargeDateInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "ChargeDate", Value = ((ChargeDateInfillType)infill).Value.ToShortDateString() });
							break;
						case "ChargePartyInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "ChargeParty", Value = ((ChargePartyInfillType)infill).Value });
							break;
						case "DateInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "Date", Value = ((DateInfillType)infill).Value.ToShortDateString() });
							break;
						case "DeedDateInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "DeedDate", Value = ((DeedDateInfillType)infill).Value });
							break;
						case "DeedExtentInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "DeedExtent", Value = ((DeedExtentInfillType)infill).Value });
							break;
						case "DeedPartyInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "DeedParty", Value = ((DeedPartyInfillType)infill).Value });
							break;
						case "DeedTypeInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "DeedType", Value = ((DeedTypeInfillType)infill).Value });
							break;
						case "ExtentOfLandInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "ExtentOfLand", Value = ((ExtentOfLandInfillType)infill).Value });
							break;
						case "MiscTextInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "MiscellaneousText", Value = ((MiscTextInfillType)infill).Value });
							break;
						case "NameInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "MiscellaneousText", Value = ((NameInfillType)infill).Value });
							break;
						case "NoteInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "MiscellaneousText", Value = ((NoteInfillType)infill).Value });
							break;
						case "OptMiscTextInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "OptionalMiscText", Value = ((OptMiscTextInfillType)infill).Value });
							break;
						case "PlansRefInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "PlansReference", Value = ((PlansRefInfillType)infill).Value });
							break;
						case "TitleNumberInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "TitleNumber", Value = ((TitleNumberInfillType)infill).Value });
							break;
						case "VerbatimTextInfillType":
							lrRestriction.Infills.Add(new KeyValuePair<string, string> { Key = "VerbatimText", Value = ((VerbatimTextInfillType)infill).Value });
							break;
					}
				}

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
				var lrAddress = new LandRegistryAddressModel { PostCode = address.PostcodeZone.Postcode.Value };
				foreach (var line in address.AddressLine.Line)
				{
					lrAddress.Lines += " " + line.Value;
				}
				addresses.Add(lrAddress);
			}
			return addresses;
		}

		private IEnumerable<string> GetIndicators(Q1RegisterEntryIndicatorsType indicators)
		{
			var lrIndicators = new List<string>();

			if (indicators.AgreedNoticeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.AgreedNoticeIndicator);
			}
			if (indicators.BankruptcyIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.BankruptcyIndicator);
			}
			if (indicators.CautionIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.CautionIndicator);
			}
			if (indicators.CCBIIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.CCBIIndicator);
			}
			if (indicators.ChargeeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.ChargeeIndicator);
			}
			if (indicators.ChargeRelatedRestrictionIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.ChargeRelatedRestrictionIndicator);
			}
			if (indicators.ChargeRestrictionIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.ChargeRestrictionIndicator);
			}
			if (indicators.CreditorsNoticeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.CreditorsNoticeIndicator);
			}
			if (indicators.DeathOfProprietorIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.DeathOfProprietorIndicator);
			}
			if (indicators.DeedOfPostponementIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.DeedOfPostponementIndicator);
			}
			if (indicators.DiscountChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.DiscountChargeIndicator);
			}
			if (indicators.EquitableChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.EquitableChargeIndicator);
			}
			if (indicators.GreenOutEntryIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.GreenOutEntryIndicator);
			}
			if (indicators.HomeRightsChangeOfAddressIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.HomeRightsChangeOfAddressIndicator);
			}
			if (indicators.HomeRightsIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.HomeRightsIndicator);
			}
			if (indicators.LeaseHoldTitleIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.LeaseHoldTitleIndicator);
			}
			if (indicators.MultipleChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.MultipleChargeIndicator);
			}
			if (indicators.NonChargeRestrictionIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.NonChargeRestrictionIndicator);
			}
			if (indicators.NotedChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.NotedChargeIndicator);
			}
			if (indicators.PricePaidIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.PricePaidIndicator);
			}
			if (indicators.PropertyDescriptionNotesIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.PropertyDescriptionNotesIndicator);
			}
			if (indicators.RentChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.RentChargeIndicator);
			}
			if (indicators.RightOfPreEmptionIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.RightOfPreEmptionIndicator);
			}
			if (indicators.ScheduleOfLeasesIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.ScheduleOfLeasesIndicator);
			}
			if (indicators.SubChargeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.SubChargeIndicator);
			}
			if (indicators.UnidentifiedEntryIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.UnidentifiedEntryIndicator);
			}
			if (indicators.UnilateralNoticeBeneficiaryIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.UnilateralNoticeBeneficiaryIndicator);
			}
			if (indicators.UnilateralNoticeIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.UnilateralNoticeIndicator);
			}
			if (indicators.VendorsLienIndicator.Value)
			{
				lrIndicators.Add(LandRegistryIndicatorText.VendorsLienIndicator);
			}

			return lrIndicators;
		}
	}
}
