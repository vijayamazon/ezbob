namespace LandRegistryLib
{
	using System;
	using System.IO;
	using System.Net;
	using log4net;

	public class LandRegistryTestApi : ILandRegistryApi
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LandRegistryApi));
		private readonly LandRegistryModelBuilder _builder = new LandRegistryModelBuilder();
		
		public LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber = null, string buildingName = null, string streetName = null, string cityName = null, string postCode = null, int customerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.Enquiry };
			using (var client = new LREnquiryServiceTestNS.PropertyDescriptionEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
				Random r = new Random();
				var a = r.Next(100);
				LREnquiryServiceTestNS.Q1AddressType q1AddressType;
				if (a < 50)
				{
					q1AddressType = new LREnquiryServiceTestNS.Q1AddressType()
						{
							BuildingName = null,
							BuildingNumber = "71", // buildingNumber
							StreetName = "Allerburn Lea", //streetName
							CityName = "Alnwick", //cityName
							PostcodeZone = null //postCode
						};
				}
				else
				{
					q1AddressType = new LREnquiryServiceTestNS.Q1AddressType()
					{
						BuildingName = null,
						BuildingNumber = "27", // buildingNumber
						StreetName = "Church Road", //streetName
						CityName = "Exeter", //cityName
						PostcodeZone = null //postCode
					};
				}

				var request = new LREnquiryServiceTestNS.RequestSearchByPropertyDescriptionV2_0Type
					{
						ID =
							new LREnquiryServiceTestNS.Q1IdentifierType
								{
									MessageID = new LREnquiryServiceTestNS.Q1TextType { Value = "012345" }
								},

						Product = new LREnquiryServiceTestNS.Q1ProductType
							{
								ExternalReference = new LREnquiryServiceTestNS.Q1ExternalReferenceType { Reference = "12345" },
								CustomerReference = new LREnquiryServiceTestNS.Q1CustomerReferenceType { Reference = "23456" },
								SubjectProperty = new LREnquiryServiceTestNS.Q1SubjectPropertyType
									{

										Address = q1AddressType
									}
							}
					};

				model.Request = XmlHelper.SerializeObject(request);
				
				LREnquiryServiceTestNS.ResponseSearchByPropertyDescriptionV2_0Type response;
				
				try
				{
					response = client.searchProperties(request);
					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);
					model.Enquery = _builder.BuildEnquiryModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Enquery = new LandRegistryEnquiryModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("resex3.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}

				return model;
			}
		}
		
		public LandRegistryDataModel EnquiryByPropertyDescriptionPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryPoll };

			using (var client = new LREnquiryPollServiceTestNS.PropertyDescriptionEnquiryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object
				var pollRequest = new LREnquiryPollServiceTestNS.PollRequestType
					{
						ID = new LREnquiryPollServiceTestNS.Q1IdentifierType
							{
								MessageID = new LREnquiryPollServiceTestNS.MessageIDTextType
									{
										Value = "pollST500681" //PollId
									}
							}
					};
				model.Request = XmlHelper.SerializeObject(pollRequest);
				LREnquiryPollServiceTestNS.ResponseSearchByPropertyDescriptionV2_0Type response;
				try
				{
					response = client.getResponse(pollRequest);
					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);
					model.Enquery = _builder.BuildEnquiryModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Enquery = new LandRegistryEnquiryModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		public LandRegistryDataModel Res(string titleNumber, int cusomerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.Res };

			// create an instance of the client
			using (var client = new LRRESServiceTestNS.OCWithSummaryV2_1ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "LandReg001"));
				// create a request object
				var request = new LRRESServiceTestNS.RequestOCWithSummaryV2_0Type
				{
					ID = new LRRESServiceTestNS.Q1IdentifierType
						{
							MessageID = new LRRESServiceTestNS.Q1TextType
								{
									Value = "170100"
								}
						},
					Product = new LRRESServiceTestNS.Q1ProductType
					{
						ExternalReference = new LRRESServiceTestNS.Q1ExternalReferenceType { Reference = "Ext_ref" },
						CustomerReference = new LRRESServiceTestNS.Q1CustomerReferenceType { Reference = "Bguser1" },
						TitleKnownOfficialCopy = new LRRESServiceTestNS.Q1TitleKnownOfficialCopyType
							{
								ContinueIfTitleIsClosedAndContinuedIndicator = new LRRESServiceTestNS.IndicatorType { Value = false },
								NotifyIfPendingFirstRegistrationIndicator = new LRRESServiceTestNS.IndicatorType { Value = false },
								NotifyIfPendingApplicationIndicator = new LRRESServiceTestNS.IndicatorType { Value = false },
								SendBackDatedIndicator = new LRRESServiceTestNS.IndicatorType { Value = false },
								ContinueIfActualFeeExceedsExpectedFeeIndicator = new LRRESServiceTestNS.IndicatorType { Value = false },
								IncludeTitlePlanIndicator = new LRRESServiceTestNS.IndicatorType { Value = true },
							},
						SubjectProperty = new LRRESServiceTestNS.Q1SubjectPropertyType { TitleNumber = new LRRESServiceTestNS.Q2TextType { Value = "GR506405" /*titleNumber*/} }
					}
				};

				model.Request = XmlHelper.SerializeObject(request);
				try
				{
					LRRESServiceTestNS.ResponseOCWithSummaryV2_1Type response = client.performOCWithSummary(request);
					
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
					catch (Exception e) {
						Log.Warn("Something went terribly not good while saving Land Registry response as attachment.", e);
					}

					try {
						if (!string.IsNullOrWhiteSpace(titleNumber))
							if (response.GatewayResponse.Results.OCSummaryData.Title.TitleNumber.Value != titleNumber)
								response.GatewayResponse.Results.OCSummaryData.Title.TitleNumber.Value += "--" + titleNumber;

						Log.DebugFormat(
							"Title number returned from TEST service is {0}",
							response.GatewayResponse.Results.OCSummaryData.Title.TitleNumber.Value
						);
					}
					catch (Exception) {
						Log.Debug("Title number returned from TEST service is: FAILED TO SHOW.");
					} // try

					response.GatewayResponse.Results.Attachment = null;
					model.Response = XmlHelper.SerializeObject(response);
					model.Res = _builder.BuildResModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Res = new LandRegistryResModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		

		public LandRegistryDataModel ResPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.ResPoll };

			// create an instance of the client
			using (var client = new LRRESPollServiceTestNS.OCWithSummaryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object
				var request = new LRRESPollServiceTestNS.PollRequestType()
				{
					ID = new LRRESPollServiceTestNS.Q1IdentifierType
					{
						MessageID = new LRRESPollServiceTestNS.MessageIDTextType
							{
								Value = "170108" //pollId
							}
					},
				};

				model.Request = XmlHelper.SerializeObject(request);
				try
				{
					LRRESPollServiceTestNS.ResponseOCWithSummaryV2_0Type response = client.getResponse(request);
					if (response.GatewayResponse.Results != null && 
						response.GatewayResponse.Results.Attachment != null &&
					    response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject != null)
					{
						//File.WriteAllBytes(string.Format("{0}_{1}.zip", pollId, DateTime.Today.Ticks), response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
						response.GatewayResponse.Results.Attachment = null;
					}

					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);
					model.Res = _builder.BuildResModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Res = new LandRegistryResModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}
	}
}
