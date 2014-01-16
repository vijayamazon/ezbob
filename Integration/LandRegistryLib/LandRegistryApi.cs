using System;

namespace LandRegistryLib
{
	using System.IO;
	using System.Net;
	using System.Xml;
	using System.Xml.Serialization;
	using log4net;

	public class LandRegistryApi
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LandRegistryApi));

		public LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber, string streetName, string cityName, string postCode, string customerId = "1")
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
									MessageID = new LREnquiryServiceNS.Q1TextType { Value = "012345" }
								},
						Product = new LREnquiryServiceNS.Q1ProductType
							{
								ExternalReference = new LREnquiryServiceNS.Q1ExternalReferenceType { Reference = string.Format("ezbob{0}", customerId) },
								CustomerReference = new LREnquiryServiceNS.Q1CustomerReferenceType { Reference = customerId },
								SubjectProperty = new LREnquiryServiceNS.Q1SubjectPropertyType
									{
										Address = new LREnquiryServiceNS.Q1AddressType
											{
												BuildingName = null,
												BuildingNumber = "27", // buildingNumber
												StreetName = "Church Road", //streetName
												CityName = "Exeter", //cityName
												PostcodeZone = null //postCode
											}
									}
							}
					};

				model.Request = SerializeObject(request);
				LREnquiryServiceNS.ResponseSearchByPropertyDescriptionV2_0Type Response;
				try
				{
					Response = client.searchProperties(request);
					model.Response = SerializeObject(Response);
					model.ResponseType = GetResponseType((int)Response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
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
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object
				var pollRequest = new LREnquiryPollServiceNS.PollRequestType
					{
						ID = new LREnquiryPollServiceNS.Q1IdentifierType
							{
								MessageID = new LREnquiryPollServiceNS.MessageIDTextType
									{
										Value = "pollST500681" //PollId
									}
							}
					};
				model.Request = SerializeObject(pollRequest);
				LREnquiryPollServiceNS.ResponseSearchByPropertyDescriptionV2_0Type Response;
				try
				{
					Response = client.getResponse(pollRequest);
					model.Response = SerializeObject(Response);
					model.ResponseType = GetResponseType((int)Response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					//File.WriteAllText("Resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		public LandRegistryDataModel Res(string titleNumber)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.RegisterExtractService };

			// create an instance of the client
			using (var client = new LRResServiceNS.OCWithSummaryV2_1ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "LandReg001"));
				// create a request object
				var request = new LRResServiceNS.RequestOCWithSummaryV2_0Type
				{
					ID = new LRResServiceNS.Q1IdentifierType
						{
							MessageID = new LRResServiceNS.Q1TextType
								{
									Value = "170100"
								}
						},
					Product = new LRResServiceNS.Q1ProductType
					{
						ExternalReference = new LRResServiceNS.Q1ExternalReferenceType { Reference = "Ext_ref" },
						CustomerReference = new LRResServiceNS.Q1CustomerReferenceType { Reference = "Bguser1" },
						TitleKnownOfficialCopy = new LRResServiceNS.Q1TitleKnownOfficialCopyType
							{
								ContinueIfTitleIsClosedAndContinuedIndicator = new LRResServiceNS.IndicatorType { Value = false },
								NotifyIfPendingFirstRegistrationIndicator = new LRResServiceNS.IndicatorType { Value = false },
								NotifyIfPendingApplicationIndicator = new LRResServiceNS.IndicatorType { Value = false },
								SendBackDatedIndicator = new LRResServiceNS.IndicatorType { Value = false },
								ContinueIfActualFeeExceedsExpectedFeeIndicator = new LRResServiceNS.IndicatorType { Value = false },
								IncludeTitlePlanIndicator = new LRResServiceNS.IndicatorType { Value = true },
							},
						SubjectProperty = new LRResServiceNS.Q1SubjectPropertyType { TitleNumber = new LRResServiceNS.Q2TextType { Value = "GR506405" /*titleNumber*/} }
					}
				};

				model.Request = SerializeObject(request);
				try
				{
					LRResServiceNS.ResponseOCWithSummaryV2_1Type response = client.performOCWithSummary(request);
					//File.WriteAllBytes(string.Format("{0}_{1}.zip", titleNumber, DateTime.Today.Ticks), Response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
					response.GatewayResponse.Results.Attachment = null;
					model.Response = SerializeObject(response);
					model.ResponseType = GetResponseType((int)response.GatewayResponse.TypeCode.Value);

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

							break;
							case LandRegistryResponseType.Success:
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
					//File.WriteAllText("Resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
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
				var request = new LRResPollServiceNS.PollRequestType()
				{
					ID = new LRResPollServiceNS.Q1IdentifierType
					{
						MessageID = new LRResPollServiceNS.MessageIDTextType
							{
								Value = "170108" //pollId
							}
					},
				};

				model.Request = SerializeObject(request);
				try
				{
					LRResPollServiceNS.ResponseOCWithSummaryV2_0Type Response = client.getResponse(request);
					if (Response.GatewayResponse.Results != null &&
						Response.GatewayResponse.Results.Attachment != null &&
						Response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject != null)
					{
						//File.WriteAllBytes(string.Format("{0}_{1}.zip", pollId, DateTime.Today.Ticks), Response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
						Response.GatewayResponse.Results.Attachment = null;
					}

					model.Response = SerializeObject(Response);
					model.ResponseType = GetResponseType((int)Response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Error = ex.Message;
					//File.WriteAllText("Resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		private static string SerializeObject<T>(T serializableObject)
		{
			if (serializableObject == null) { return null; }

			try
			{
				var xmlDocument = new XmlDocument();
				var serializer = new XmlSerializer(serializableObject.GetType());
				string xmlString;
				using (var stream = new MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlString = xmlDocument.OuterXml;
					stream.Close();
				}
				return xmlString;
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("{0}", ex);
			}
			return null;
		}

		private static LandRegistryResponseType GetResponseType(int value)
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
	}
}
