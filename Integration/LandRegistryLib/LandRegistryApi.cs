using System;

namespace LandRegistryLib
{
	using System.IO;
	using System.Xml;
	using System.Xml.Serialization;

	public class LandRegistryApi
	{
		public LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber, string streetName, string cityName, string postCode)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryByPropertyDescription };
			// create an instance of the client
			using (var client = new LREnquiryServiceTestNS.PropertyDescriptionEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object

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
										Address = new LREnquiryServiceTestNS.Q1AddressType()
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
				LREnquiryServiceTestNS.ResponseSearchByPropertyDescriptionV2_0Type response;
				try
				{
					response = client.searchProperties(request);
					model.Response = SerializeObject(response);
					model.ResponseType = GetResponseType((int) response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					model.Error = ex.Message;
					//File.WriteAllText("resex3.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}

				return model;
			}
		}
		
		public LandRegistryDataModel EnquiryByPropertyDescriptionPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryByPropertyDescriptionPoll };

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
				model.Request = SerializeObject(pollRequest);
				LREnquiryPollServiceTestNS.ResponseSearchByPropertyDescriptionV2_0Type response;
				try
				{
					response = client.getResponse(pollRequest);
					model.Response = SerializeObject(response);
					model.ResponseType = GetResponseType((int)response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					model.Error = ex.Message;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		public LandRegistryDataModel Res(string titleNumber)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.RegisterExtractService };

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

				model.Request = SerializeObject(request);
				try
				{
					LRRESServiceTestNS.ResponseOCWithSummaryV2_1Type response = client.performOCWithSummary(request);
					//File.WriteAllBytes(string.Format("{0}_{1}.zip", titleNumber, DateTime.Today.Ticks), response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
					response.GatewayResponse.Results.Attachment = null;
					model.Response = SerializeObject(response);
					model.ResponseType = GetResponseType((int)response.GatewayResponse.TypeCode.Value);
					
				}
				catch (Exception ex)
				{
					model.Error = ex.Message;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		public LandRegistryDataModel ResPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.RegisterExtractServicePoll };

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

				model.Request = SerializeObject(request);
				try
				{
					LRRESPollServiceTestNS.ResponseOCWithSummaryV2_0Type response = client.getResponse(request);
					if (response.GatewayResponse.Results != null && 
						response.GatewayResponse.Results.Attachment != null &&
					    response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject != null)
					{
						File.WriteAllBytes(string.Format("{0}_{1}.zip", pollId, DateTime.Today.Ticks),
						                   response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
						response.GatewayResponse.Results.Attachment = null;
					}
					
					model.Response = SerializeObject(response);
					model.ResponseType = GetResponseType((int)response.GatewayResponse.TypeCode.Value);
				}
				catch (Exception ex)
				{
					model.Error = ex.Message;
					//File.WriteAllText("resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		private string SerializeObject<T>(T serializableObject)
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
				//Log exception here
			}
			return null;
		}

		private LandRegistryResponseType GetResponseType(int value)
		{
			LandRegistryResponseType type;
			switch (value)
			{
				case 1:
					type = LandRegistryResponseType.Poll;
					break;
				case 2:
					type = LandRegistryResponseType.Error;
					break;
				case 3:
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
