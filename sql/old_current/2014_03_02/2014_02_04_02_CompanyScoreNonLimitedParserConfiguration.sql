UPDATE ConfigurationVariables SET Value = '[
	{
		"Name": "Locality Resolution List Data Group",

		"PathToParent": "./REQUEST/DG02",

		"MetaData": {
			"ID": "DG02"
		},

		"Fields": [
			{ "SourcePath": "./ERRORCODE", "Targets": [
				{ "Name": "Error code" },
				{
					"Name": "Error code",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"E001": "More details required to derive RMC",
							"E002": "RMC could not be uniquely identified",
							"E003": "Postcode is in invalid format",
							"E004": "Postcode does not exist",
							"E005": "District does not exist",
							"E006": "Town does not exist",
							"E007": "Address could not be resolved, more than one data inconsistency",
							"E008": "Postcode partially matched, omit or resupply correct postcode",
							"W006": "No address supplied"
						}
					}
				}
			]}
		]
	},
	{
		"Name": "Business Targeter Selection List Entry",

		"PathToParent": "./REQUEST/DT11",

		"MetaData": {
			"ID": "DT11"
		},

		"Fields": [
			{
				"SourcePath": "./LEGALSTATUS", "Targets": [{
					"Name": "Legal status",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"L": "Limited",
							"N": "Non-limited"
						}
					}
				}]
			},
			{
				"SourcePath": "./BUSINESSSTATUS", "Targets": [{
					"Name": "Business status",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"A": "Active",
							"D": "Dissolved"
						}
					}
				}]
			},
			{ "SourcePath": "./MATCHSCORE", "Targets": [{ "Name": "Score calculated from matching processing set to spaces if suppressed" }] },
			{ "SourcePath": "./BUSREFNUM", "Targets": [{ "Name": "Registered number or Non-limited key" }] },
			{ "SourcePath": "./BUSNAME", "Targets": [{ "Name": "Business name of matched business" }] },
			{ "SourcePath": "./ADDRLINE1", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./ADDRLINE2", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDRLINE3", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDRLINE4", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDRLINE5", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./POSTCODE", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] },
			{
				"SourcePath": "./SICCODETYPE", "Targets": [{
					"Name": "SIC code type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"A": 1980,
							"B": 1992
						}
					}
				}]
			},
			{ "SourcePath": "./SICCODE", "Targets": [{ "Name": "SIC code" }] },
			{ "SourcePath": "./SICCODEDESC", "Targets": [{ "Name": "Description of the SIC code" }] },
			{ "SourcePath": "./BUSACTIVITIES", "Targets": [{ "Name": "(Non-limited only) Derived from the Thomson category code" }] },
			{ "SourcePath": "./MATCHEDPHONENUM", "Targets": [{ "Name": "Telephone number of matched business" }] },
			{ "SourcePath": "./MATCHEPHONETYPE", "Targets": [{ "Name": "Matched phone type" }] },
			{ "SourcePath": "./MATCHEDBUSNAME", "Targets": [{ "Name": "Matched business name (populated IF a name other than the current registered name contributed to the matching)" }] },
			{
				"SourcePath": "./MATCHEDBUSNAMETYPE", "Targets": [{
					"Name": "Matched business name type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"RC": "Registered Name (current)",
							"RP": "Previous registered name",
							"TC": "Trading name (limited)",
							"NL": "Trading name (non-limited)"
						}
					}
				}]
			},
			{ "SourcePath": "./MATCHEDPOSTCODE", "Targets": [{ "Name": "Postcode of matched business" }] },
			{ "SourcePath": "./MATCHEDLOCALITY", "Targets": [{ "Name": "Populated if a locality other than the registered locality contributed to the search" }] },
			{
				"SourcePath": "./MATCHEDLOCALITYTYPE", "Targets": [{
					"Name": "Matched locality type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"RC": "Registered address (current)",
							"RP": "Previous registered address",
							"TC": "Trading address (limited)"
						}
					}
				}]
			}
		]
	},
	{
		"Name": "Non-Limited Report General Details",

		"PathToParent": "./REQUEST/DN10",

		"MetaData": {
			"ID": "DN10"
		},

		"Fields": [
			{ "SourcePath": "./EARLIESTKNOWNDATE-YYYY", "Targets": [{ "Name": "Earliest known date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./EARLIESTKNOWNDATE-MM", "Targets": [{ "Name": "Earliest known date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./EARLIESTKNOWNDATE-DD", "Targets": [{ "Name": "Earliest known date", "Position": 1, "Prefix": " " }] },

			{ "SourcePath": "./DATEOWNSHPCOMMD-YYYY", "Targets": [{ "Name": "Date owndership commenced", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DATEOWNSHPCOMMD-MM", "Targets": [{ "Name": "Date owndership commenced", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DATEOWNSHPCOMMD-DD", "Targets": [{ "Name": "Date owndership commenced", "Position": 1, "Prefix": " " }] },

			{ "SourcePath": "./DATEOWNSHPTERMD-YYYY", "Targets": [{ "Name": "Date ownership ceased", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DATEOWNSHPTERMD-MM", "Targets": [{ "Name": "Date ownership ceased", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DATEOWNSHPTERMD-DD", "Targets": [{ "Name": "Date ownership ceased", "Position": 1, "Prefix": " " }] },

			{ "SourcePath": "./LATESTUPDATE-YYYY", "Targets": [{ "Name": "Date of latest update", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./LATESTUPDATE-MM", "Targets": [{ "Name": "Date of latest update", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./LATESTUPDATE-DD", "Targets": [{ "Name": "Date of latest update", "Position": 1, "Prefix": " " }] },

			{ "SourcePath": "./BUSINESSNAME", "Targets": [{ "Name": "Business name" }] },

			{ "SourcePath": "./BUSADDR1", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./BUSADDR2", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./BUSADDR3", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./BUSADDR4", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./BUSADDR5", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./BUSPOSTCODE", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] },

			{ "SourcePath": "./TELEPHONENUM", "Targets": [{ "Name": "Telephone number" }] },

			{ "SourcePath": "./PRINCIPALACTIVITIES", "Targets": [{ "Name": "Principal activity" }] }
		]
	},
	{
		"Name": "Non-Limited Report General Details - SIC Codes",

		"PathToParent": "./REQUEST/DN10/SICCODES",

		"MetaData": {
			"ID": "DN10-SIC"
		},

		"Fields": [
			{ "SourcePath": "./SICCODE1992", "Targets": [{ "Name": "SIC code" }] }
		]
	},
	{
		"Name": "Non-Limited Report General Details - SIC Descriptions",

		"PathToParent": "./REQUEST/DN10/SICDESCS",

		"MetaData": {
			"ID": "DN10-SICDESC"
		},

		"Fields": [
			{ "SourcePath": "./SICDESC1992", "Targets": [{ "Name": "Description" }] }
		]
	},
	{
		"Name": "Non-Limited Bankruptcy Details",

		"PathToParent": "./REQUEST/DN11",

		"MetaData": {
			"ID": "DN11"
		},

		"Fields": [
			{ "SourcePath": "./GAZETTE-YYYY", "Targets": [{ "Name": "Gazette date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./GAZETTE-MM", "Targets": [{ "Name": "Gazette date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./GAZETTE-DD", "Targets": [{ "Name": "Gazette date", "Position": 1, "Prefix": " " }] },
			{
				"SourcePath": "./BANKRUPTCYTYPE", "Targets": [{
					"Name": "Bankruptcy type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"BO": "Bankruptcy Order",
							"SEQ": "Sequestration",
							"OD": "Order Of Discharge"
						}
					}
				}]
			},
			{ "SourcePath": "./BANKRUPTCYNAME", "Targets": [{ "Name": "Bankruptcy name" }] },
			{ "SourcePath": "./BANKRUPTCYADDR1", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./BANKRUPTCYADDR2", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKRUPTCYADDR3", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKRUPTCYADDR4", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKRUPTCYADDR5", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKRUPTCYPOSTCODE", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Non-Limited Bankruptcy Summary ",

		"PathToParent": "./REQUEST/DN12",

		"MetaData": {
			"ID": "DN12"
		},

		"Fields": [
			{ "SourcePath": "./MBANKRUPTCYCOUNTOWNSHP", "Targets": [{ "Name": "Bankruptcy count during ownership" }] },
			{ "SourcePath": "./MMOSTRECBANKRUPCY", "Targets": [{ "Name": "Age of most recent bankruptcy during ownership (Months)" }] },
			{ "SourcePath": "./ABANKRUPTCYCOUNTOWNSHP", "Targets": [{ "Name": "Associated bankruptcy count during ownership" }] },
			{ "SourcePath": "./AMOSTRECBANKRUPCY", "Targets": [{ "Name": "Age of most recent associated bankruptcy during ownership (Months)" }] }
		]
	},
	{
		"Name": "Non-Limited CCJ Details",

		"PathToParent": "./REQUEST/DN13",

		"MetaData": {
			"ID": "DN13"
		},

		"Fields": [
			{ "SourcePath": "./RECORDTYPE", "Targets": [{ "Name": "Record type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"M": "Main",
							"A": "Associated",
							"P": "Pool"
						}
					}
				}]
			},
			{ "SourcePath": "./JUDGDATE-YYYY", "Targets": [{ "Name": "Judgement date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./JUDGDATE-MM", "Targets": [{ "Name": "Judgement date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./JUDGDATE-DD", "Targets": [{ "Name": "Judgement date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./SATFLAG", "Targets": [{ "Name": "Satisfaction flag",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Satisfied",
							"N": "Not satisfied"
						}
					}
				}]
			},
			{ "SourcePath": "./SATDATE-YYYY", "Targets": [{ "Name": "Satisfaction date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./SATDATE-MM", "Targets": [{ "Name": "Satisfaction date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./SATDATE-DD", "Targets": [{ "Name": "Satisfaction date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./JUDGTYPE", "Targets": [{ "Name": "Judgment type",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"JG": "Judgment",
							"SS": "Satisfied Judgment",
							"DO": "Discovery Order",
							"CU": "Certificate Of Unenforceability"
						}
					}
				}]
			},
			{ "SourcePath": "./JUDGAMOUNT", "Targets": [{ "Name": "Judgment amount" }] },
			{ "SourcePath": "./COURT", "Targets": [{ "Name": "Court" }] },
			{ "SourcePath": "./CASENUM", "Targets": [{ "Name": "Case number" }] },
			{ "SourcePath": "./NUMJUDGNAMES", "Targets": [{ "Name": "Number of judgment names" }] },
			{ "SourcePath": "./NUMTRADNAMES", "Targets": [{ "Name": "Number of trading names" }] },
			{ "SourcePath": "./LENJUDGNAME", "Targets": [{ "Name": "Length of judgment name" }] },
			{ "SourcePath": "./LENTRADNAME", "Targets": [{ "Name": "Length of trading name" }] },
			{ "SourcePath": "./LENJUDGADDR", "Targets": [{ "Name": "Length of judgment address" }] },
			{ "SourcePath": "./JUDGADDR1", "Targets": [{ "Name": "Judgement address" }] },
			{ "SourcePath": "./JUDGADDR2", "Targets": [{ "Name": "Judgement address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./JUDGADDR3", "Targets": [{ "Name": "Judgement address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./JUDGADDR4", "Targets": [{ "Name": "Judgement address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./JUDGADDR5", "Targets": [{ "Name": "Judgement address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./JUDGPOSTCODE", "Targets": [{ "Name": "Judgement address", "Position": 5, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Non-Limited CCJ Details - Judgement Details",

		"PathToParent": "./REQUEST/DN13/JUDGDETS",

		"MetaData": {
			"ID": "DN13-JUDGDETS"
		},

		"Fields": [
			{ "SourcePath": "./JUDGREGAGAINST", "Targets": [{ "Name": "Judgment registered against" }] }
		]
	},
	{
		"Name": "Non-Limited CCJ Details - Trading Details",

		"PathToParent": "./REQUEST/DN13/TRADDETS",

		"MetaData": {
			"ID": "DN13-TRADDETS"
		},

		"Fields": [
			{ "SourcePath": "./TRADNAME", "Targets": [{ "Name": "Trading names" }] },
			{ "SourcePath": "./TRADINDICATOR", "Targets": [
				{ "Name": "Trading indicator" },
				{
					"Name": "Trading indicator",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"F": "Formerly trading as",
							"P": "Previously trading as",
							"T": "Trading as"
						}
					}
				}
			]}
		]
	},
	{
		"Name": "Non-Limited CCJ Summary",

		"PathToParent": "./REQUEST/DN14",

		"MetaData": {
			"ID": "DN14"
		},

		"Fields": [
			{ "SourcePath": "./MAGEMOSTRECJUDGSINCEOWNSHP", "Targets": [{ "Name": "Age of most recent judgment during ownership (Months)" }] },
			{ "SourcePath": "./MTOTJUDGCOUNTLST12MNTHS", "Targets": [{ "Name": "Total judgment count in last 12 months" }] },
			{ "SourcePath": "./MTOTJUDGVALUELST12MNTHS", "Targets": [{ "Name": "Total judgment value in last 12 months" }] },
			{ "SourcePath": "./MTOTJUDGCOUNTLST13TO24MNTHS", "Targets": [{ "Name": "Total judgment count in last 13 to 24 months" }] },
			{ "SourcePath": "./MTOTJUDGVALUELST13TO24MNTHS", "Targets": [{ "Name": "Total judgment value in last 13 to 24 months" }] },
			{ "SourcePath": "./AVALMOSTRECJUDGSINCEOWNSHP", "Targets": [{ "Name": "Value of most recent associated judgment during ownership" }] },
			{ "SourcePath": "./ATOTJUDGCOUNTLST12MNTHS", "Targets": [{ "Name": "Total associated judgment count in last 12 months" }] },
			{ "SourcePath": "./ATOTJUDGVALUELST12MNTHS", "Targets": [{ "Name": "Total associated judgment value in last 12 months" }] },
			{ "SourcePath": "./ATOTJUDGCOUNTLST13TO24MNTHS", "Targets": [{ "Name": "Total associated judgment count in last 13 to 24 months" }] },
			{ "SourcePath": "./ATOTJUDGVALUELST13TO24MNTHS", "Targets": [{ "Name": "Total associated judgment value in last 13 to 24 months" }] }
		]
	},
	{
		"Name": "Non-Limited Business Previous Searches",

		"PathToParent": "./REQUEST/DN17",

		"MetaData": {
			"ID": "DN17"
		},

		"Fields": [
			{ "SourcePath": "./PREVSEARCHDATE-YYYY", "Targets": [{ "Name": "Previous search date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./PREVSEARCHDATE-MM", "Targets": [{ "Name": "Previous search date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./PREVSEARCHDATE-DD", "Targets": [{ "Name": "Previous search date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./ENQUIRYTYPE", "Targets": [
				{ "Name": "Enquiry type" },
				{
					"Name": "Enquiry type",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Z": "Business Confirmation",
							"Y": "Business Profile",
							"X": "Credit Profile",
							"W": "Full Profile",
							"D": "e-series Gold",
							"E": "e-series Silver",
							"F": "e-series Bronze",
							"N": "Commercial Autoscore Application",
							"O": "Commercial Autoscore Reprocess Application",
							"6": "Written Report",
							"C": "CPU Link Enquiry",
							"Q": "Credit Card Report",
							"X": "XML Bespoke Delivery"
						}
					}
				}
			]},
			{ "SourcePath": "./CREDITREQD", "Targets": [{ "Name": "Credit Required" }] }
		]
	}
]
' WHERE Name = 'CompanyScoreNonLimitedParserConfiguration'
GO
