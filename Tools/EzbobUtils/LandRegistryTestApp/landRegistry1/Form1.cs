using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace landRegistry1
{
	using System.IO;
	using System.Xml;
	using System.Xml.Serialization;
	using LandRegistryRESNS;

	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			// create an instance of the client
			using (var client = new landRegistry1.LandRegistryServiceNS.DaylistEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "LandReg001"));
				// create a request object
				var request = new landRegistry1.LandRegistryServiceNS.RequestDaylistEnquiryV2_0Type
					{
						ID = new LandRegistryServiceNS.Q1IdentifierType { MessageID = new LandRegistryServiceNS.Q1TextType { Value = "testsuccessmanyresults" } },
						Product = new LandRegistryServiceNS.Q1ProductType
							{
								ContinueIfTitleIsClosedAndContinuedIndicator = new landRegistry1.LandRegistryServiceNS.IndicatorType { Value = true },
								ExternalReference = new LandRegistryServiceNS.Q1ExternalReferenceType { Reference = "DotNetQuickStartReference" },
								SubjectProperty = new LandRegistryServiceNS.Q1SubjectPropertyType { TitleNumber = new landRegistry1.LandRegistryServiceNS.Q2TextType { Value = "DN100" } }
							}
					};
				// populate it
				// submit the request
				
				landRegistry1.LandRegistryServiceNS.ResponseDaylistEnquiryV2_0Type response = client.daylistEnquiry(request);
			}
			throw new Exception("Application Processed");
		}

		public void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) { return; }

			try
			{
				var xmlDocument = new XmlDocument();
				var serializer = new XmlSerializer(serializableObject.GetType());
				using (var stream = new MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
					stream.Close();
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			// create an instance of the client
			using (var client = new LandRegistryEnquiryByPropertyDescriptionNS.PropertyDescriptionEnquiryV2_0ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "landreg001"));
				// create a request object
				var request = new LandRegistryEnquiryByPropertyDescriptionNS.RequestSearchByPropertyDescriptionV2_0Type
				{
					ID = new LandRegistryEnquiryByPropertyDescriptionNS.Q1IdentifierType { MessageID = new LandRegistryEnquiryByPropertyDescriptionNS.Q1TextType { Value = "pollST500681" } },
					Product = new LandRegistryEnquiryByPropertyDescriptionNS.Q1ProductType
						{
							ExternalReference = new LandRegistryEnquiryByPropertyDescriptionNS.Q1ExternalReferenceType { Reference = "123" },
							CustomerReference = new LandRegistryEnquiryByPropertyDescriptionNS.Q1CustomerReferenceType { Reference = "321" },
							SubjectProperty = new LandRegistryEnquiryByPropertyDescriptionNS.Q1SubjectPropertyType
								{
									Address = new LandRegistryEnquiryByPropertyDescriptionNS.Q1AddressType()
										{
											BuildingName = "27",
											BuildingNumber = "27",
											StreetName = "Church Road",
											CityName = "Bristol",
											PostcodeZone = "TQ56 4HY"
										}
								}
						}
				};
				
				SerializeObject(request, "req.xml");

				LandRegistryEnquiryByPropertyDescriptionNS.ResponseSearchByPropertyDescriptionV2_0Type response = client.searchProperties(request);
			}
			throw new Exception("Application Processed");
		}

		private void button3_Click(object sender, EventArgs e)
		{
			// create an instance of the client
			using (var client = new LandRegistryRESNS.OCWithSummaryV2_1ServiceClient())
			{
				client.ChannelFactory.Endpoint.Behaviors.Add(new HMLRBGMessageEndpointBehavior("BGUser001", "LandReg001"));
				// create a request object
				var request = new LandRegistryRESNS.RequestOCWithSummaryV2_0Type
				{
					ID = new LandRegistryRESNS.Q1IdentifierType { MessageID = new LandRegistryRESNS.Q1TextType { Value = "170100" } },
					Product = new LandRegistryRESNS.Q1ProductType
					{
						ExternalReference = new LandRegistryRESNS.Q1ExternalReferenceType { Reference = "Ext_ref" },
						CustomerReference = new LandRegistryRESNS.Q1CustomerReferenceType { Reference = "Bguser1" },
						TitleKnownOfficialCopy = new LandRegistryRESNS.Q1TitleKnownOfficialCopyType
							{
								ContinueIfTitleIsClosedAndContinuedIndicator = new LandRegistryRESNS.IndicatorType { Value = false },
								NotifyIfPendingFirstRegistrationIndicator = new LandRegistryRESNS.IndicatorType { Value = false },
								NotifyIfPendingApplicationIndicator = new LandRegistryRESNS.IndicatorType { Value = false },
								SendBackDatedIndicator = new LandRegistryRESNS.IndicatorType { Value = false },
								ContinueIfActualFeeExceedsExpectedFeeIndicator = new LandRegistryRESNS.IndicatorType { Value = false },
								IncludeTitlePlanIndicator = new LandRegistryRESNS.IndicatorType { Value = true },
							},
						SubjectProperty = new LandRegistryRESNS.Q1SubjectPropertyType
						{
							TitleNumber = new LandRegistryRESNS.Q2TextType { Value = "GR506405" }
						}
					}
				};

				SerializeObject(request, "req3.xml");

				LandRegistryRESNS.ResponseOCWithSummaryV2_1Type response = client.performOCWithSummary(request);
				SerializeObject(response, "res3.xml");
				File.WriteAllBytes("file.zip", response.GatewayResponse.Results.Attachment.EmbeddedFileBinaryObject.Value);
			}
			throw new Exception("Application Processed");
		}
	}
}
