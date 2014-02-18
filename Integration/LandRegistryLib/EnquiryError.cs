namespace LandRegistryLib
{
	using System.Collections.Generic;

	public class EnquiryError
	{
		public string RuleDescription { get; set; }
		public string BgCode { get; set; }
		public string MessageId { get; set; }
		public string MessageText { get; set; }
	}

	public class EnquiryErrors
	{
		public List<EnquiryError> Errors { get; set; }
		public EnquiryErrors()
		{
			Errors = new List<EnquiryError>
				{
					new EnquiryError
						{
							RuleDescription = "BRL-ISBG-001",
							BgCode = "bg.invalid.property.search.criteria",
							MessageId = "MSG-BG-001",
							MessageText =
								"Insufficient address details. Please provide flat/house and postcode OR flat, street and town OR house, street and town."
						},
					new EnquiryError
						{
							RuleDescription = "BRL-ISBG-002",
							BgCode = "bg.postcode.invalid",
							MessageId = "MSG-BG-004",
							MessageText = "Please provide valid postcode"
						},
					new EnquiryError
						{
							RuleDescription = "BRL-ISBG-003",
							BgCode = "bg.properties.nopropertyfound",
							MessageId = "MSG-BG-003",
							MessageText =
								"No title number has been identified from the data supplied. This does not necessarily mean that a register of a title does not exist but only that insufficient data has matched."
						},
					new EnquiryError
						{
							RuleDescription = "BRL-ISBG-003",
							BgCode = "bg.properties.toomanyproperties",
							MessageId = "MSG-BG-002",
							MessageText =
								"The property address you entered has matched with a large number of properties on our database. Please request again with refined address details."
						}
				};
		}
	}
}
