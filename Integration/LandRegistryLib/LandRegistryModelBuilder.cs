﻿namespace LandRegistryLib {
	using System;
	using System.Collections.Generic;
	using LREnquiryServiceNS;
	using LRResServiceNS;
	using log4net;
	using Q1AddressType = LRResServiceNS.Q1AddressType;

	public class LandRegistryModelBuilder {
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(LandRegistryModelBuilder));

		public LandRegistryResponseType GetResponseType(int value) {
			LandRegistryResponseType type;
			switch (value) {
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

		public LandRegistryEnquiryModel BuildEnquiryModel(string responseXml) {
			var response = XmlHelper.DeserializeObject<ResponseSearchByPropertyDescriptionV2_0Type>(responseXml);
			//XmlHelper.XmlDeserializeFromString<ResponseSearchByPropertyDescriptionV2_0Type>(responseXml);
			return BuildEnquiryModel(response);
		}

		public LandRegistryEnquiryModel BuildEnquiryModel(ResponseSearchByPropertyDescriptionV2_0Type response) {
			var model = new LandRegistryEnquiryModel { Titles = new List<LandRegistryEnquiryTitle>() };
			if (response.GatewayResponse.Acknowledgement != null) {
				var ack = response.GatewayResponse.Acknowledgement.AcknowledgementDetails;
				model.Acknowledgement = new LandRegistryAcknowledgementModel {
					PollDate = ack.ExpectedResponseDateTime != null ? DateTime.SpecifyKind(ack.ExpectedResponseDateTime.Value, DateTimeKind.Utc) : new DateTime(1900, 01, 01),
					Description = ack.MessageDescription != null ? ack.MessageDescription.Value : null,
					UniqueId = ack.UniqueID != null ? ack.UniqueID.Value : null
				};
			} else if (response.GatewayResponse.Rejection != null) {
				var rej = response.GatewayResponse.Rejection.RejectionResponse;
				model.Rejection = new LandRegistryRejectionModel {
					Reason = rej.Reason != null ? rej.Reason.Value : "Some error occurred",
					OtherDescription = rej.OtherDescription != null ? rej.OtherDescription.Value : null
				};
			} else {
				var data = response.GatewayResponse.Results;
				foreach (var title in data.Title) {
					var lrTitle = new LandRegistryEnquiryTitle {
						TitleNumber = title.TitleNumber == null ? null : title.TitleNumber.Value,
						Postcode =
							(title.Address.PostcodeZone == null || title.Address.PostcodeZone.Postcode == null)
								? null
								: title.Address.PostcodeZone.Postcode.Value,
						BuildingName = title.Address.BuildingName == null ? null : title.Address.BuildingName.Value,
						CityName = title.Address.CityName == null ? null : title.Address.CityName.Value,
						BuildingNumber = title.Address.BuildingNumber == null ? null : title.Address.BuildingNumber.Value,
						StreetName = title.Address.StreetName == null ? null : title.Address.StreetName.Value,
						SubBuildingName = title.Address.SubBuildingName == null ? null : title.Address.SubBuildingName.Value
					};
					model.Titles.Add(lrTitle);
				}
			}
			return model;
		}

		public LandRegistryResModel BuildResModel(string responseXml, string titleNumber = "titleNumber") {
			var response = XmlHelper.XmlDeserializeFromString<ResponseOCWithSummaryV2_1Type>(responseXml);
			return BuildResModel(response, titleNumber);
		}

		public LandRegistryResModel BuildResModel(ResponseOCWithSummaryV2_1Type response, string titleNumber = "titleNumber") {
			var model = new LandRegistryResModel();
			if (response.GatewayResponse.Acknowledgement != null) {
				model.Acknowledgement = GetAcknowledgement(response.GatewayResponse.Acknowledgement.AcknowledgementDetails);
				model.TitleNumber = titleNumber;
			} else if (response.GatewayResponse.Rejection != null) {
				model.Rejection = GetRejection(response.GatewayResponse.Rejection.RejectionResponse);
				model.TitleNumber = titleNumber;
			} else {
				var data = response.GatewayResponse.Results.OCSummaryData;
				model.Indicators = new List<string>();
				try {
					FillSummaryData(model, data);
				} catch (Exception ex) {
					ms_oLog.ErrorFormat("Error parsing lr data for title {0} {1}\n{2}", titleNumber, model.TitleNumber, ex);
					model.Indicators.Add("Error parsing the data (tell dev)");
				}
			}
			return model;
		}

		private void FillSummaryData(LandRegistryResModel model, Q1OCSummaryDataType data) {
			model.TitleNumber = data.Title.TitleNumber.Value;

			if (data.Title.CommonholdIndicator != null && data.Title.CommonholdIndicator.Value == false) {
				model.Indicators.Add(LandRegistryIndicatorText.CommonholdIndicator);
			}

			if (data.PricePaidEntry != null && data.PricePaidEntry.EntryDetails != null) {
				model.PricePaidInfills = GetInfills(data.PricePaidEntry.EntryDetails.Infills);
			}

			model.PropertyAddresses = GetAddresses(data.PropertyAddress);

			model.Proprietorship = new LandRegistryProprietorshipModel {
				CurrentProprietorshipDate = data.Proprietorship.CurrentProprietorshipDate != null ? DateTime.SpecifyKind(data.Proprietorship.CurrentProprietorshipDate.Value, DateTimeKind.Utc) : (DateTime?)null,
				ProprietorshipParties = new List<ProprietorshipPartyModel>()
			};

			if (data.Proprietorship.Items != null) {
				for (int i = 0; i < data.Proprietorship.Items.Length; ++i) {
					model.Proprietorship.ProprietorshipParties.Add(GetProprietorship(data.Proprietorship.Items[i], data.Proprietorship.ItemsElementName[i]));
				}
			}

			model.Restrictions = new List<LandRegistryRestrictionModel>();

			if (data.RestrictionDetails != null) {
				foreach (var restriction in data.RestrictionDetails) {
					model.Restrictions.Add(GetRestriction(restriction));
				}
			}

			model.Indicators.AddRange(GetIndicators(data.RegisterEntryIndicators));

			model.Charges = new List<LandRegistryChargeModel>();
			if (data.Charge != null && data.Charge.Length > 0) {
				foreach (var charge in data.Charge) {
					model.Charges.Add(GetCharge(charge));
				}
			}
		}

		private LandRegistryRejectionModel GetRejection(LRResServiceNS.Q1RejectionResponseType rej) {
			return new LandRegistryRejectionModel {
				Reason = rej.Reason != null ? rej.Reason.Value : "Some error occurred",
				OtherDescription = rej.OtherDescription != null ? rej.OtherDescription.Value : null
			};
		}

		private LandRegistryAcknowledgementModel GetAcknowledgement(LRResServiceNS.Q1AcknowledgementDetailsType ack) {
			 return new LandRegistryAcknowledgementModel {
					PollDate = ack.ExpectedResponseDateTime != null ? ack.ExpectedResponseDateTime.Value : new DateTime(1900, 01, 01),
					Description = ack.MessageDescription != null ? ack.MessageDescription.Value : null,
					UniqueId = ack.UniqueID != null ? ack.UniqueID.Value : null
				};
		}

		private LandRegistryChargeModel GetCharge(Q1ChargeDetailsType charge) {
			var lrCharge = new LandRegistryChargeModel {
				ChargeDate = charge.ChargeDate != null ? DateTime.SpecifyKind(charge.ChargeDate.Value, DateTimeKind.Utc) : new DateTime(1900, 1, 1),
				Description = charge.RegisteredCharge.EntryDetails.EntryText != null ? charge.RegisteredCharge.EntryDetails.EntryText.Value : string.Empty
			};
			var lrProprietorship = new LandRegistryProprietorshipModel {
				ProprietorshipParties = new List<ProprietorshipPartyModel>()
			};

			foreach (var proprietorship in charge.ChargeProprietor.ChargeeParty) {
				var lrProprietor = new ProprietorshipPartyModel();
				if (proprietorship.Item.GetType() == typeof(Q1PrivateIndividualType)) {
					lrProprietor.ProprietorshipPartyType = "Private Individual";
					lrProprietor.PrivateIndividualForename =
						((Q1PrivateIndividualType)proprietorship.Item).Name.ForenamesName != null ? ((Q1PrivateIndividualType)proprietorship.Item).Name.ForenamesName.Value : string.Empty;
					lrProprietor.PrivateIndividualSurname =
						((Q1PrivateIndividualType)proprietorship.Item).Name.SurnameName != null ? ((Q1PrivateIndividualType)proprietorship.Item).Name.SurnameName.Value : string.Empty;
				} else if (proprietorship.Item.GetType() == typeof(Q1OrganizationType)) {
					lrProprietor.ProprietorshipPartyType = "Organization";
					lrProprietor.CompanyName = ((Q1OrganizationType)proprietorship.Item).Name != null ? ((Q1OrganizationType)proprietorship.Item).Name.Value : string.Empty;
				}
				lrProprietor.ProprietorshipAddresses = GetAddresses(proprietorship.Address);
				lrProprietorship.ProprietorshipParties.Add(lrProprietor);
			}
			lrCharge.Proprietorship = lrProprietorship;
			return lrCharge;
		}

		private LandRegistryRestrictionModel GetRestriction(Q1RestrictionEntryType restriction) {
			var lrRestriction = new LandRegistryRestrictionModel();
			switch (restriction.ItemElementName) {
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
			lrRestriction.EntryText = restriction.Item.EntryDetails.EntryText != null ? restriction.Item.EntryDetails.EntryText.Value : string.Empty;
			lrRestriction.EntryNumber = restriction.Item.EntryDetails.EntryNumber != null ? restriction.Item.EntryDetails.EntryNumber.Value : string.Empty;
			if (restriction.Item.EntryDetails.Item.GetType() == typeof(ScheduleCodeType)) {
				var code = (RestictionScheduleCode)(int)((ScheduleCodeType)restriction.Item.EntryDetails.Item);
				lrRestriction.ScheduleCode = code.DescriptionAttr();
			} else if (restriction.Item.EntryDetails.Item.GetType() == typeof(SubRegisterCodeType)) {
				var code = (RestrictionSubRegisterCode)(int)((SubRegisterCodeType)restriction.Item.EntryDetails.Item);
				lrRestriction.SubRegisterCode = code.DescriptionAttr();
			}

			lrRestriction.Infills = GetInfills(restriction.Item.EntryDetails.Infills);
			return lrRestriction;
		}

		private ProprietorshipPartyModel GetProprietorship(Q1PartyType proprietorship, ItemsChoiceType type) {
			var lrProprietorship = new ProprietorshipPartyModel();
			if (type == ItemsChoiceType.CautionerParty) {
				lrProprietorship.ProprietorshipType = "Cautioner Party";
			} else if (type == ItemsChoiceType.RegisteredProprietorParty) {
				lrProprietorship.ProprietorshipType = "Registered Proprietorship Party";
			}
			if (proprietorship.Item.GetType() == typeof(Q1PrivateIndividualType)) {
				lrProprietorship.ProprietorshipPartyType = "Private Individual";
				lrProprietorship.PrivateIndividualForename =
					((Q1PrivateIndividualType)proprietorship.Item).Name.ForenamesName != null ? ((Q1PrivateIndividualType)proprietorship.Item).Name.ForenamesName.Value : string.Empty;
				lrProprietorship.PrivateIndividualSurname =
					((Q1PrivateIndividualType)proprietorship.Item).Name.SurnameName != null ? ((Q1PrivateIndividualType)proprietorship.Item).Name.SurnameName.Value : string.Empty;
			} else if (proprietorship.Item.GetType() == typeof(Q1OrganizationType)) {
				lrProprietorship.ProprietorshipPartyType = "Organization";
				lrProprietorship.CompanyRegistrationNumber =
					((Q1OrganizationType)proprietorship.Item).CompanyRegistrationNumber != null
						? ((Q1OrganizationType)proprietorship.Item).CompanyRegistrationNumber.Value : string.Empty;
				lrProprietorship.CompanyName = ((Q1OrganizationType)proprietorship.Item).Name != null ? ((Q1OrganizationType)proprietorship.Item).Name.Value : string.Empty;
			}
			lrProprietorship.ProprietorshipAddresses = GetAddresses(proprietorship.Address);
			return lrProprietorship;
		}

		private List<LandRegistryAddressModel> GetAddresses(Q1AddressType[] propertyAddress) {
			var addresses = new List<LandRegistryAddressModel>();
			if (propertyAddress == null) {
				return addresses;
			}

			foreach (var address in propertyAddress) {
				var lrAddress = new LandRegistryAddressModel {
					PostCode =
						address.PostcodeZone != null && address.PostcodeZone.Postcode != null ? address.PostcodeZone.Postcode.Value : null
				};
				if (address.AddressLine != null && address.AddressLine.Line != null) {
					foreach (var line in address.AddressLine.Line) {
						lrAddress.Lines += " " + line.Value;
					}
					addresses.Add(lrAddress);
				}
			}
			return addresses;
		}

		private List<KeyValuePair<string, string>> GetInfills(IEnumerable<object> infills) {
			var lrInfills = new List<KeyValuePair<string, string>>();

			if (infills == null)
				return lrInfills;

			foreach (var infill in infills) {

				switch (infill.GetType().Name) {
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

		private IEnumerable<string> GetIndicators(Q1RegisterEntryIndicatorsType indicators) {
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
	}
}
