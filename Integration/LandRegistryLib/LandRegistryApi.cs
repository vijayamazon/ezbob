namespace LandRegistryLib
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using log4net;

	public class LandRegistryApi : ILandRegistryApi
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LandRegistryApi));
		private readonly LandRegistryModelBuilder _builder = new LandRegistryModelBuilder();

		/// <summary>
		/// Please provide flat/house and postcode OR flat, street and town OR house, street and town.
		/// </summary>
		/// <param name="buildingNumber">flat/house</param>
		/// <param name="streetName">street</param>
		/// <param name="cityName">town</param>
		/// <param name="postCode">postcode</param>
		/// <param name="customerId">customerId</param>
		/// <returns></returns>
		public LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber = null, string streetName = null, string cityName = null, string postCode = null, int customerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryByPropertyDescription };
			using (var client = new LREnquiryServiceNS.PropertyDescriptionEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("SDulman3000", "Ezbob2013$LR"));
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

				var request = new LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type
					{
						ID =
							new LREnquiryServiceNS.Q1IdentifierType
								{
									MessageID = new LREnquiryServiceNS.Q1TextType { Value = "ENQREQ" + customerId }
								},
						Product = new LREnquiryServiceNS.Q1ProductType
							{
								ExternalReference = new LREnquiryServiceNS.Q1ExternalReferenceType { Reference = string.Format("ezbob{0}", customerId) },
								CustomerReference = new LREnquiryServiceNS.Q1CustomerReferenceType { Reference = customerId.ToString(CultureInfo.InvariantCulture) },
								SubjectProperty = new LREnquiryServiceNS.Q1SubjectPropertyType
									{
										Address = new LREnquiryServiceNS.Q1AddressType
											{
												BuildingName = null,
												BuildingNumber = buildingNumber, // buildingNumber
												StreetName = streetName, //streetName
												CityName = cityName, //cityName
												PostcodeZone = postCode //postCode
											}
									}
							}
					};

				model.Request = XmlHelper.SerializeObject(request);
				try
				{
					LREnquiryServiceNS.ResponseSearchByPropertyDescriptionV2_0Type response = client.searchProperties(request);
					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);

					switch (model.ResponseType)
					{
						case LandRegistryResponseType.Acknowledgement:
							model.Acknowledgement = new LandRegistryAcknowledgementModel
							{
								PollDate = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.ExpectedResponseDateTime.Value,
								Description = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.MessageDescription.Value,
								UniqueId = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.UniqueID.Value
							};
							break;
						case LandRegistryResponseType.Rejection:
							model.Rejection = new LandRegistryRejectionModel
							{
								Reason = response.GatewayResponse.Rejection.RejectionResponse.Reason.Value,
								OtherDescription = response.GatewayResponse.Rejection.RejectionResponse.OtherDescription.Value
							};
							break;
						case LandRegistryResponseType.Success:
							model.Enquery = _builder.BuildEnquiryModel(model.Response);
							break;
						case LandRegistryResponseType.Unkown:
						default:
							break;
					}

				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("Resex3.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}

				return model;
			}
		}

		public LandRegistryDataModel EnquiryByPropertyDescriptionPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryByPropertyDescriptionPoll };

			using (var client = new LREnquiryPollServiceNS.PropertyDescriptionEnquiryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("SDulman3000", "Ezbob2013$LR"));
				// create a request object
				var pollRequest = new LREnquiryPollServiceNS.PollRequestType
					{
						ID = new LREnquiryPollServiceNS.Q1IdentifierType
							{
								MessageID = new LREnquiryPollServiceNS.MessageIDTextType
									{
										Value = pollId //PollId
									}
							}
					};
				model.Request = XmlHelper.SerializeObject(pollRequest);
				try
				{
					LREnquiryPollServiceNS.ResponseSearchByPropertyDescriptionV2_0Type response = client.getResponse(pollRequest);
					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);

					switch (model.ResponseType)
					{
						case LandRegistryResponseType.Acknowledgement:
							model.Acknowledgement = new LandRegistryAcknowledgementModel
							{
								PollDate = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.ExpectedResponseDateTime.Value,
								Description = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.MessageDescription.Value,
								UniqueId = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.UniqueID.Value
							};
							break;
						case LandRegistryResponseType.Rejection:
							model.Rejection = new LandRegistryRejectionModel
							{
								Reason = response.GatewayResponse.Rejection.RejectionResponse.Reason.Value,
								OtherDescription = response.GatewayResponse.Rejection.RejectionResponse.OtherDescription.Value
							};
							break;
						case LandRegistryResponseType.Success:
							model.Enquery = _builder.BuildEnquiryModel(model.Response);
							break;
						case LandRegistryResponseType.Unkown:
						default:
							break;
					}

				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("Resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		public LandRegistryDataModel Res(string titleNumber, int customerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.RegisterExtractService };

			// create an instance of the client
			using (var client = new LRResServiceNS.OCWithSummaryV2_1ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("SDulman3000", "Ezbob2013$LR"));
				// create a request object
				var request = new LRResServiceNS.RequestOCWithSummaryV2_0Type
				{
					ID = new LRResServiceNS.Q1IdentifierType
						{
							MessageID = new LRResServiceNS.Q1TextType
								{
									Value = "RESREQ" + customerId
								}
						},
					Product = new LRResServiceNS.Q1ProductType
					{
						ExternalReference = new LRResServiceNS.Q1ExternalReferenceType { Reference = "ezbob" + customerId },
						CustomerReference = new LRResServiceNS.Q1CustomerReferenceType { Reference = customerId.ToString(CultureInfo.InvariantCulture) },
						TitleKnownOfficialCopy = new LRResServiceNS.Q1TitleKnownOfficialCopyType
							{
								ContinueIfTitleIsClosedAndContinuedIndicator = new LRResServiceNS.IndicatorType { Value = false },
								NotifyIfPendingFirstRegistrationIndicator = new LRResServiceNS.IndicatorType { Value = false },
								NotifyIfPendingApplicationIndicator = new LRResServiceNS.IndicatorType { Value = false },
								SendBackDatedIndicator = new LRResServiceNS.IndicatorType { Value = false },
								ContinueIfActualFeeExceedsExpectedFeeIndicator = new LRResServiceNS.IndicatorType { Value = true },
								IncludeTitlePlanIndicator = new LRResServiceNS.IndicatorType { Value = true },
							},
						SubjectProperty = new LRResServiceNS.Q1SubjectPropertyType { TitleNumber = new LRResServiceNS.Q2TextType { Value = titleNumber } },
					}
				};

				model.Request = XmlHelper.SerializeObject(request);
				try
				{
					LRResServiceNS.ResponseOCWithSummaryV2_1Type response = client.performOCWithSummary(request);

					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);

					try
					{
						if (model.ResponseType == LandRegistryResponseType.Success)
						{
							model.Attachment = new LandRegistryAttachmentModel
								{
									AttachmentContent = response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value,
									FileName = string.Format("{0}_{1}.zip", titleNumber, DateTime.Today.Ticks),
									FilePath = string.Format("c:\\temp\\landregistry\\{0}_{1}.zip", titleNumber, DateTime.Today.Ticks)
								};
							File.WriteAllBytes(string.Format("c:\\temp\\landregistry\\{0}_{1}.zip", titleNumber, DateTime.UtcNow.Ticks),
											   response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
							response.GatewayResponse.Results.Attachment = null;
						}
					}
					catch { }

					model.Response = XmlHelper.SerializeObject(response);



					switch (model.ResponseType)
					{
						case LandRegistryResponseType.Acknowledgement:
							model.Acknowledgement = new LandRegistryAcknowledgementModel
								{
									PollDate = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.ExpectedResponseDateTime.Value,
									Description = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.MessageDescription.Value,
									UniqueId = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.UniqueID.Value
								};
							break;
						case LandRegistryResponseType.Rejection:
							model.Rejection = new LandRegistryRejectionModel
								{
									Reason = response.GatewayResponse.Rejection.RejectionResponse.Reason.Value,
									OtherDescription = response.GatewayResponse.Rejection.RejectionResponse.OtherDescription.Value
								};
							break;
						case LandRegistryResponseType.Success:
							model.Res = _builder.BuildResModel(response);
							break;
						case LandRegistryResponseType.Unkown:
						default:
							break;
					}


				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					model.ResponseType = LandRegistryResponseType.Rejection;
				}
			}
			return model;
		}

		public LandRegistryDataModel ResPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.RegisterExtractServicePoll };

			// create an instance of the client
			using (var client = new LRResPollServiceNS.OCWithSummaryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object
				var request = new LRResPollServiceNS.PollRequestType
				{
					ID = new LRResPollServiceNS.Q1IdentifierType
					{
						MessageID = new LRResPollServiceNS.MessageIDTextType
							{
								Value = pollId //pollId
							}
					},
				};

				model.Request = XmlHelper.SerializeObject(request);

				try
				{
					LRResPollServiceNS.ResponseOCWithSummaryV2_0Type response = client.getResponse(request);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);

					if (model.ResponseType == LandRegistryResponseType.Success)
					{
						try
						{
							model.Attachment = new LandRegistryAttachmentModel
								{
									AttachmentContent = response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value,
									FileName = string.Format("{0}_{1}.zip", pollId, DateTime.Today.Ticks),
									FilePath = string.Format("c:\\temp\\landregistry\\{0}_{1}.zip", pollId, DateTime.Today.Ticks)
								};
							File.WriteAllBytes(string.Format("c:\\temp\\landregistry\\{0}_{1}.zip", pollId, DateTime.Today.Ticks), response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
							response.GatewayResponse.Results.Attachment = null;
						}
						catch (Exception) { }
					}

					model.Response = XmlHelper.SerializeObject(response);


					switch (model.ResponseType)
					{
						case LandRegistryResponseType.Acknowledgement:
							model.Acknowledgement = new LandRegistryAcknowledgementModel
							{
								PollDate = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.ExpectedResponseDateTime.Value,
								Description = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.MessageDescription.Value,
								UniqueId = response.GatewayResponse.Acknowledgement.AcknowledgementDetails.UniqueID.Value
							};
							break;
						case LandRegistryResponseType.Rejection:
							model.Rejection = new LandRegistryRejectionModel
							{
								Reason = response.GatewayResponse.Rejection.RejectionResponse.Reason.Value,
								OtherDescription = response.GatewayResponse.Rejection.RejectionResponse.OtherDescription.Value
							};
							break;
						case LandRegistryResponseType.Success:
							model.Res = _builder.BuildResModel(model.Response);
							break;
						case LandRegistryResponseType.Unkown:
						default:
							break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					model.ResponseType = LandRegistryResponseType.Rejection;
				}
			}
			return model;
		}



		
	}
}
