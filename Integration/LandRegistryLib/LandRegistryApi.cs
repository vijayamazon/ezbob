namespace LandRegistryLib
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using log4net;

	public class LandRegistryApi : ILandRegistryApi
	{
		private readonly string _userName;
		private readonly string _password;
		private readonly string _filePath;
		private static readonly ILog Log = LogManager.GetLogger(typeof(LandRegistryApi));
		private readonly LandRegistryModelBuilder _builder = new LandRegistryModelBuilder();

		public LandRegistryApi(string userName, string password, string filePath)
		{
			_userName = userName;
			_password = password;
			_filePath = filePath;
		}

		/// <summary>
		/// Please provide flat/house and postcode OR flat, street and town OR house, street and town.
		/// </summary>
		/// <param name="buildingNumber">flat/house number</param>
		/// <param name="buildingName">flat/house name</param>
		/// <param name="streetName">street</param>
		/// <param name="cityName">town</param>
		/// <param name="postCode">postcode</param>
		/// <param name="customerId">customerId</param>
		/// <returns></returns>
		public LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber = null, string buildingName = null, string streetName = null, string cityName = null, string postCode = null, int customerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.Enquiry };
			using (var client = new LREnquiryServiceNS.PropertyDescriptionEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior(_userName, _password));
				ServicePointManager.Expect100Continue = true;

				var address = new LREnquiryServiceNS.Q1AddressType();
				if (!string.IsNullOrWhiteSpace(buildingName))
				{
					address.BuildingName = buildingName;
				}
				if (!string.IsNullOrWhiteSpace(buildingNumber))
				{
					address.BuildingNumber = buildingNumber;
				}
				if (!string.IsNullOrWhiteSpace(streetName))
				{
					address.StreetName = streetName;
				}
				if (!string.IsNullOrWhiteSpace(cityName))
				{
					address.CityName = cityName;
				}
				if (!string.IsNullOrWhiteSpace(postCode))
				{
					address.PostcodeZone = postCode;
				}

				var request = new LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type
					{
						ID =
							new LREnquiryServiceNS.Q1IdentifierType
								{
									MessageID = new LREnquiryServiceNS.Q1TextType { Value = "ENQREQ" + customerId + "-" + Guid.NewGuid().ToString("N") }
								},
						Product = new LREnquiryServiceNS.Q1ProductType
							{
								ExternalReference = new LREnquiryServiceNS.Q1ExternalReferenceType { Reference = string.Format("ezbob{0}", customerId) },
								CustomerReference = new LREnquiryServiceNS.Q1CustomerReferenceType { Reference = customerId.ToString(CultureInfo.InvariantCulture) },
								SubjectProperty = new LREnquiryServiceNS.Q1SubjectPropertyType
									{
										Address = address
									}
							}
					};

				model.Request = XmlHelper.SerializeObject(request);
				try
				{
					LREnquiryServiceNS.ResponseSearchByPropertyDescriptionV2_0Type response = client.searchProperties(request);
					model.Response = XmlHelper.SerializeObject(response);
					model.ResponseType = _builder.GetResponseType((int)response.GatewayResponse.TypeCode.Value);
					model.Enquery = _builder.BuildEnquiryModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Enquery = new LandRegistryEnquiryModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					model.Response = XmlHelper.SerializeObject(new LREnquiryServiceNS.ResponseSearchByPropertyDescriptionV2_0Type
						{
							GatewayResponse = new LREnquiryServiceNS.Q1GatewayResponseType
								{
									TypeCode = new LREnquiryServiceNS.ProductResponseCodeType
										{
											Value = LREnquiryServiceNS.ProductResponseCodeContentType.Item20
										},
									Rejection = new LREnquiryServiceNS.Q1RejectionType
										{
											RejectionResponse = new LREnquiryServiceNS.Q1RejectionResponseType
												{
													Reason = new LREnquiryServiceNS.TextType1 { Value = ex.Message }
												}
										}
								}
						});
				}

				return model;
			}
		}

		public LandRegistryDataModel EnquiryByPropertyDescriptionPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.EnquiryPoll };

			using (var client = new LREnquiryPollServiceNS.PropertyDescriptionEnquiryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior(_userName, _password));
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
					model.Enquery = _builder.BuildEnquiryModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Enquery = new LandRegistryEnquiryModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
					//File.WriteAllText("Resex1c.xml", string.Format("{0} \n {1}", ex.Message, ex.StackTrace));
				}
			}
			return model;
		}

		/// <summary>
		/// MessageID : minLength="5" maxLength="50" pattern="[a-zA-Z0-9][a-zA-Z0-9\-]*"
		/// </summary>
		/// <param name="titleNumber">Title Number</param>
		/// <param name="customerId">Customer Id</param>
		/// <returns>LR parsed data model</returns>
		public LandRegistryDataModel Res(string titleNumber, int customerId = 1)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.Res };

			// create an instance of the client
			using (var client = new LRResServiceNS.OCWithSummaryV2_1ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior(_userName, _password));
				// create a request object

				string sMessageID = "RESREQ" + customerId + "-" + Guid.NewGuid().ToString("N");

				var request = new LRResServiceNS.RequestOCWithSummaryV2_0Type
				{
					ID = new LRResServiceNS.Q1IdentifierType
						{
							MessageID = new LRResServiceNS.Q1TextType
								{
									Value = sMessageID
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
									FilePath = string.Format("{2}{0}_{1}.zip", titleNumber, DateTime.Today.Ticks, _filePath)
								};
							File.WriteAllBytes(string.Format("{2}{0}_{1}.zip", titleNumber, DateTime.UtcNow.Ticks, _filePath),
											   response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
							response.GatewayResponse.Results.Attachment = null;
						}
					}
					catch (Exception e)
					{
						Log.Warn("Something went terribly not good while saving Land Registry response as attachment.", e);
					}

					try
					{
						Log.DebugFormat(
							"Title number returned from PROD service is {0}",
							response.GatewayResponse.Results.OCSummaryData.Title.TitleNumber.Value
						);
					}
					catch (Exception)
					{
						Log.Debug("Title number returned from PROD service is: FAILED TO SHOW.");
					} // try

					model.Response = XmlHelper.SerializeObject(response);
					model.Res = _builder.BuildResModel(response, titleNumber);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Res = new LandRegistryResModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;

					model.Response = XmlHelper.SerializeObject(new LRResServiceNS.ResponseOCWithSummaryV2_1Type
					{
						GatewayResponse = new LRResServiceNS.Q1GatewayResponseType
						{
							TypeCode = new LRResServiceNS.ProductResponseCodeType
							{
								Value = LRResServiceNS.ProductResponseCodeContentType.Item20
							},
							Rejection = new LRResServiceNS.Q1RejectionType
							{
								RejectionResponse = new LRResServiceNS.Q1RejectionResponseType
								{
									Reason = new LRResServiceNS.TextType { Value = ex.Message }
								}
							}
						}
					});
				}
			}
			return model;
		}

		public LandRegistryDataModel ResPoll(string pollId)
		{
			var model = new LandRegistryDataModel { RequestType = LandRegistryRequestType.ResPoll };

			// create an instance of the client
			using (var client = new LRResPollServiceNS.OCWithSummaryV2_0PollServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior(_userName, _password));
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
									FilePath = string.Format("{2}{0}_{1}.zip", pollId, DateTime.Today.Ticks, _filePath)
								};
							File.WriteAllBytes(string.Format("{2}{0}_{1}.zip", pollId, DateTime.Today.Ticks, _filePath), response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
							response.GatewayResponse.Results.Attachment = null;
						}
						catch (Exception) { }
					}

					model.Response = XmlHelper.SerializeObject(response);
					model.Res = _builder.BuildResModel(model.Response);
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("{0}", ex);
					model.Res = new LandRegistryResModel { Rejection = new LandRegistryRejectionModel { Reason = ex.Message } };
					model.ResponseType = LandRegistryResponseType.Rejection;
				}
			}
			return model;
		}

	}
}
