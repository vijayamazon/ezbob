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
	},
	{
		"Name": "Non Limited Business CIFAS Details",

		"PathToParent": "./REQUEST/DN23",

		"MetaData": {
			"ID": "DN23"
		},

		"Fields": [
			{ "SourcePath": "./FRAUDCATEGORY", "Targets": [
				{ "Name": "Fraud category" },
				{
					"Name": "Fraud category",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"01": "Providing a false name and a true address.",
							"02": "Providing or using the name and particulars of another person.",
							"03": "Providing or using a genuine name and address, but one or more material falsehoods in personal details followed by a serious misuse of the credit or other facility and/or non-payment.",
							"04": "Providing or using a genuine name and address, but one or more material falsehoods in personal details.",
							"05": "Disposal/selling on of goods obtained on credit and failing to settle the finance agreement.",
							"06": "Opening an account for the purpose of fraud."
						}
					}
				}
			]},
			{ "SourcePath": "./SUPPLIERNAME", "Targets": [{ "Name": "Supplier name" }] }
		]
	},
	{
		"Name": "Non-Limited Payment Performance Full Details",

		"PathToParent": "./REQUEST/DN26",

		"MetaData": {
			"ID": "DN26"
		},

		"Fields": [
			{ "SourcePath": "./NUMACCSPLACEDFORCOLLTN", "Targets": [{ "Name": "Number of accounts placed for collection" }] },
			{ "SourcePath": "./VALACCSPLACEDFORCOLLTN", "Targets": [{ "Name": "Value of accounts placed for collection" }] },
			{ "SourcePath": "./NUMACCSPLACEDFORCOLLTNLST2YRS", "Targets": [{ "Name": "Number of accounts placed for collection in last 2 years" }] },
			{ "SourcePath": "./AVDBT0-100", "Targets": [{ "Name": "Average days beyond terms for ?0-?100" }] },
			{ "SourcePath": "./AVDBT101-1000", "Targets": [{ "Name": "Average days beyond terms for ?101-?1,000" }] },
			{ "SourcePath": "./AVDBT1001-10000", "Targets": [{ "Name": "Average days beyond terms for ?1,001-?10,000" }] },
			{ "SourcePath": "./AVDBTGREATERTHAN10000", "Targets": [{ "Name": "Average days beyond terms for >?10,000" }] },
			{ "SourcePath": "./AVDBTLST3MNTHSDATARTND", "Targets": [{ "Name": "Average days beyond terms for last 3 months of data returned" }] },
			{ "SourcePath": "./AVDBTLST6MNTHSDATARTND", "Targets": [{ "Name": "Average days beyond terms for last 6 months of data returned" }] },
			{ "SourcePath": "./AVDBTLST12MNTHSDATARTND", "Targets": [{ "Name": "Average days beyond terms for last 12 months of data returned" }] },
			{ "SourcePath": "./CURRAVDEBT", "Targets": [{ "Name": "Current average debt" }] },
			{ "SourcePath": "./AVDEBTLST3MNTHS", "Targets": [{ "Name": "Average debt in last 3 months" }] },
			{ "SourcePath": "./AVDEBTLST12MNTHS", "Targets": [{ "Name": "Average debt in last 12 months" }] }
		]
	},
	{
		"Name": "Non-Limited Payment Performance Full Details - Common Terms",

		"PathToParent": "./REQUEST/DN26/COMMONTERMS",

		"MetaData": {
			"ID": "DN26-COMMONTERMS"
		},

		"Fields": [
			{ "SourcePath": "./COMMONTERMSCODE", "Targets": [{ "Name": "Common terms code" }] },
			{ "SourcePath": "./COMMONTERMSDBT", "Targets": [{ "Name": "Common terms days beyond terms" }] }
		]
	},
	{
		"Name": "Non-Limited Preference Service Block",

		"PathToParent": "./REQUEST/DN36",

		"MetaData": {
			"ID": "DN36"
		},

		"Fields": [
			{ "SourcePath": "./TELEPHONENUM", "Targets": [{ "Name": "Telephone number" }] }
		]
	},
	{
		"Name": "Non-Limited Commercial Delphi Block",

		"PathToParent": "./REQUEST/DN73",

		"MetaData": {
			"ID": "DN73"
		},

		"Fields": [
			{ "SourcePath": "./SEARCHTYPE", "Targets": [
				{ "Name": "Search type" },
				{
					"Name": "Search type",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"P": "Proprietor only",
							"B": "Business only",
							"J": "Business & proprietor"
						}
					}
				}
			]},
			{ "SourcePath": "./NLCDSCORE", "Targets": [{ "Name": "NL Commercial Delphi Score" }] },
			{ "SourcePath": "./CREDITRATING", "Targets": [{ "Name": "Credit Rating" }] },
			{ "SourcePath": "./CREDITLIMIT", "Targets": [{ "Name": "Credit Limit" }] },
			{ "SourcePath": "./PDSCORE", "Targets": [{ "Name": "Probability of Default Score" }] },
			{ "SourcePath": "./STABILITYODDS", "Targets": [{ "Name": "Stability Odds" }] },
			{ "SourcePath": "./RISKBAND", "Targets": [{ "Name": "Risk Band" }] },
			{ "SourcePath": "./NUMPROPSSEARCHED", "Targets": [{ "Name": "Number of Proprietors Searched" }] },
			{ "SourcePath": "./NUMPROPSFOUND", "Targets": [{ "Name": "Number of Proprietors Found" }] }
		]
	},
	{
		"Name": "Non-Limited Individual Director Details",

		"PathToParent": "./REQUEST/DD11",

		"MetaData": {
			"ID": "DD11",
			"DisplayDirection": "horizontal"
		},

		"Fields": [
			{ "SourcePath": "./FGNADDRFLAG", "Targets": [
				{ "Name": "Foreign Address Flag" },
				{
					"Name": "Foreign Address Flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Foreign",
							"N": "Not foreign",
							"M": "Unknown"
						}
					}
				}
			]},
			{ "SourcePath": "./DIRCOMPFLAG", "Targets": [
				{ "Name": "Director Is a Company Flag" },
				{
					"Name": "Director Is a Company Flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Director is a company",
							"N": "Director is not a company"
						}
					}
				}
			]},
			{ "SourcePath": "./DOB-YYYY", "Targets": [{ "Name": "Date of birth", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DOB-MM", "Targets": [{ "Name": "Date of birth", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DOB-DD", "Targets": [{ "Name": "Date of birth", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./FORENAME", "Targets": [{ "Name": "Name", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./MIDNAME1", "Targets": [{ "Name": "Name", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./MIDNAME2", "Targets": [{ "Name": "Name", "Position": 3, "Prefix": " " }] },
			{ "SourcePath": "./SURNAME", "Targets": [{ "Name": "Name", "Position": 4, "Prefix": " " }] },
			{ "SourcePath": "./COMPNAME", "Targets": [{ "Name": "Company name" }] },
			{ "SourcePath": "./HOUSENAME", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./HOUSENUM", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./STREET", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./DISTRICT", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./TOWN", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./COUNTY", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] },
			{ "SourcePath": "./POSTCODE", "Targets": [{ "Name": "Address", "Position": 6, "Prefix": "\n" }] },
			{ "SourcePath": "./NATION", "Targets": [{ "Name": "Nationality" }] }
		]
	},
	{
		"Name": "Non-Limited Individual Director Previous Address",

		"PathToParent": "./REQUEST/DD12",

		"MetaData": {
			"ID": "DD12",
			"DisplayDirection": "horizontal"
		},

		"Fields": [
			{ "SourcePath": "./FGNADDRFLAG", "Targets": [
				{ "Name": "Foreign Address Flag" },
				{
					"Name": "Foreign Address Flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Foreign",
							"N": "Not foreign",
							"M": "Unknown"
						}
					}
				}
			]},
			{ "SourcePath": "./RMC", "Targets": [{ "Name": "RMC" }] },
			{ "SourcePath": "./CHANGED-YYYY", "Targets": [{ "Name": "Date changed", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./CHANGED-MM", "Targets": [{ "Name": "Date changed", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./CHANGED-DD", "Targets": [{ "Name": "Date changed", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./LENGVARAREA", "Targets": [{ "Name": "Length of variable area" }] },
			{ "SourcePath": "./HOUSENAME", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./HOUSENUM", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./STREET", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./DISTRICT", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./TOWN", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./COUNTY", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] },
			{ "SourcePath": "./POSTCODE", "Targets": [{ "Name": "Address", "Position": 6, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Non-Limited Individual Director Current Directorships",

		"PathToParent": "./REQUEST/DD14",

		"MetaData": {
			"ID": "DD14",
			"DisplayDirection": "horizontal"
		},

		"Fields": [
			{ "SourcePath": "./APPT-YYYY", "Targets": [{ "Name": "Appointment date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./APPT-MM", "Targets": [{ "Name": "Appointment date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./APPT-DD", "Targets": [{ "Name": "Appointment date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./MONASDIR", "Targets": [{ "Name": "Months as director" }] },
			{ "SourcePath": "./LATEVENTCODE", "Targets": [
				{ "Name": "Latest event code" },
				{
					"Name": "Latest event code",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"05": "Compulsory Liquidation",
							"10": "Creditors Voluntary Liquidation",
							"15": "Members Voluntary Liquidation",
							"20": "Liquidation of Unknown Type",
							"25": "Winding up Order",
							"30": "Administrator Appointed",
							"35": "Administration Order Discharged",
							"40": "Administrative Receiver Appointed",
							"45": "Receiver Appointment",
							"50": "Administrative Receiver Cessation",
							"55": "Receiver Cessation",
							"60": "Voluntary Arrangement Approved",
							"65": "Voluntary Arrangement Completed",
							"70": "Voluntary Arrangement Revoked",
							"75": "Voluntary Arrangement Suspended",
							"80": "Company Reinstated"
						}
					}
				}
			]},
			{ "SourcePath": "./POSITION", "Targets": [{ "Name": "Position" }] },
			{ "SourcePath": "./OCCUPATION", "Targets": [{ "Name": "Occupation" }] },
			{ "SourcePath": "./QUAL", "Targets": [{ "Name": "Qualifications" }] },
			{ "SourcePath": "./SHARES", "Targets": [{ "Name": "Shares" }] },
			{ "SourcePath": "./COMPNUM", "Targets": [{ "Name": "Company number" }] },
			{ "SourcePath": "./COMPNAME", "Targets": [{ "Name": "Company name" }] }
		]
	},
	{
		"Name": "Non-Limited Individual Director Previous Directorships",

		"PathToParent": "./REQUEST/DD15",

		"MetaData": {
			"ID": "DD15",
			"DisplayDirection": "horizontal"
		},

		"Fields": [
			{ "SourcePath": "./APPT-YYYY", "Targets": [{ "Name": "Appointment date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./APPT-MM", "Targets": [{ "Name": "Appointment date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./APPT-DD", "Targets": [{ "Name": "Appointment date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./RESIGN-YYYY", "Targets": [{ "Name": "Resignation date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./RESIGN-MM", "Targets": [{ "Name": "Resignation date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./RESIGN-DD", "Targets": [{ "Name": "Resignation date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./MONASDIR", "Targets": [{ "Name": "Months as director" }] },
			{ "SourcePath": "./LATEVENTCODE", "Targets": [
				{ "Name": "Latest event code" },
				{
					"Name": "Latest event code",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"05": "Compulsory Liquidation",
							"10": "Creditors Voluntary Liquidation",
							"15": "Members Voluntary Liquidation",
							"20": "Liquidation of Unknown Type",
							"25": "Winding up Order",
							"30": "Administrator Appointed",
							"35": "Administration Order Discharged",
							"40": "Administrative Receiver Appointed",
							"45": "Receiver Appointment",
							"50": "Administrative Receiver Cessation",
							"55": "Receiver Cessation",
							"60": "Voluntary Arrangement Approved",
							"65": "Voluntary Arrangement Completed",
							"70": "Voluntary Arrangement Revoked",
							"75": "Voluntary Arrangement Suspended",
							"80": "Company Reinstated"
						}
					}
				}
			]},
			{ "SourcePath": "./POSITION", "Targets": [{ "Name": "Position" }] },
			{ "SourcePath": "./OCCUPATION", "Targets": [{ "Name": "Occupation" }] },
			{ "SourcePath": "./QUAL", "Targets": [{ "Name": "Qualifications" }] },
			{ "SourcePath": "./SHARES", "Targets": [{ "Name": "Shares" }] },
			{ "SourcePath": "./COMPNUM", "Targets": [{ "Name": "Company number" }] },
			{ "SourcePath": "./COMPNAME", "Targets": [{ "Name": "Company name" }] }
		]
	},
	{
		"Name": "Individual Director CIFAS Details",

		"PathToParent": "./REQUEST/DD23",

		"MetaData": {
			"ID": "DD23"
		},

		"Fields": [
			{ "SourcePath": "./REFERENCE", "Targets": [{ "Name": "Reference" }] },
			{ "SourcePath": "./FRAUDCATEGORY", "Targets": [
				{ "Name": "Fraud category" },
				{
					"Name": "Fraud category",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"01": "Providing a false name and a true address.",
							"02": "Providing or using the name and particulars of another person.",
							"03": "Providing or using a genuine name and address, but one or more material falsehoods in personal details followed by a serious misuse of the credit or other facility and/or non-payment.",
							"04": "Providing or using a genuine name and address, but one or more material falsehoods in personal details.",
							"05": "Disposal/selling on of goods obtained on credit and failing to settle the finance agreement.",
							"06": "Opening an account for the purpose of fraud."
						}
					}
				}
			]},
			{ "SourcePath": "./SUPPLIERNAME", "Targets": [{ "Name": "Supplier name" }] },
			{ "SourcePath": "./DATESUPPLIED-YYYY", "Targets": [{ "Name": "Date supplied", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DATESUPPLIED-MM", "Targets": [{ "Name": "Date supplied", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DATESUPPLIED-DD", "Targets": [{ "Name": "Date supplied", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./EXPIRYDATE-YYYY", "Targets": [{ "Name": "Expiry date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./EXPIRYDATE-MM", "Targets": [{ "Name": "Expiry date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./EXPIRYDATE-DD", "Targets": [{ "Name": "Expiry date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./OTHERADDRFLG", "Targets": [
				{ "Name": "Other address flag" },
				{
					"Name": "Other address flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Fraud has occurred at other addresses"
						}
					}
				}
			]},
			{ "SourcePath": "./NAME", "Targets": [{ "Name": "Name" }] },
			{ "SourcePath": "./ADDR1", "Targets": [{ "Name": "Address" }] },
			{ "SourcePath": "./ADDR2", "Targets": [{ "Name": "Address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDR3", "Targets": [{ "Name": "Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDR4", "Targets": [{ "Name": "Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDR5", "Targets": [{ "Name": "Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./POSTCODE", "Targets": [{ "Name": "Address", "Position": 5, "Prefix": "\n" }] }
		]
	}
]
' WHERE Name = 'CompanyScoreNonLimitedParserConfiguration'
GO
