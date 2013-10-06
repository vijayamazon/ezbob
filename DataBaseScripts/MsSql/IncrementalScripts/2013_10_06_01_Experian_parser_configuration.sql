DELETE FROM ConfigurationVariables WHERE Name IN ('CompanyScoreParserConfiguration', 'DirectorInfoParserConfiguration')
GO

INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('CompanyScoreParserConfiguration','[
	{
		"Name": "Limited Company Identification",

		"PathToParent": "./REQUEST/DL12",

		"MetaData": {
			"ID": "DL12"
		},

		"Fields": [
			{ "SourcePath": "./REGNUMBER", "Targets": [{ "Name": "Registered Number" }] },
			{
				"SourcePath": "./LEGALSTATUS",
				"Targets": [
					{ "Name": "Legal Status" },
					{
						"Name": "Legal Status",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"1": "Private Unlimited",
								"2": "Private Limited",
								"3": "PLC",
								"4": "Old Public Company",
								"5": "Private Company Limited by Guarantee (Exempt from using word Limited)",
								"6": "Limited Partnership",
								"7": "Private Limited Company Without Share Capital",
								"8": "Company Converted / Closed",
								"9": "Private Unlimited Company Without Share Capital",
								"0": "Other",
								"A": "Private Company Limited by Shares (Exempt from using word Limited)"
							}
						}
					}
				]
			},
			{
				"SourcePath": "./DATEINCORP-YYYY", "Targets": [{ "Name": "Incorporation Date", "Position": 2, "Prefix": " " }]
			},
			{
				"SourcePath": "./DATEINCORP-MM", "Targets": [{ "Name": "Incorporation Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }]
			},
			{
				"SourcePath": "./DATEINCORP-DD", "Targets": [{ "Name": "Incorporation Date", "Position": 1, "Prefix": " " }]
			},
			{
				"SourcePath": "./DATEDISSVD-YYYY", "Targets": [{ "Name": "Dissolution Date", "Position": 2, "Prefix": " " }]
			},
			{
				"SourcePath": "./DATEDISSVD-MM", "Targets": [{ "Name": "Dissolution Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }]
			},
			{
				"SourcePath": "./DATEDISSVD-DD", "Targets": [{ "Name": "Dissolution Date", "Position": 1, "Prefix": " " }]
			},
			{ "SourcePath": "./COMPANYNAME", "Targets": [{ "Name": "Company Name" }] },
			{ "SourcePath": "./REGADDR1", "Targets": [{ "Name": "Office Address", "Position": 1 }] },
			{ "SourcePath": "./REGADDR2", "Targets": [{ "Name": "Office Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./REGADDR3", "Targets": [{ "Name": "Office Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./REGADDR4", "Targets": [{ "Name": "Office Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./REGPOSTCODE", "Targets": [{ "Name": "Office Address", "Position": 5, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Limited Company Commerical Delphi Score",

		"PathToParent": "./REQUEST/DL76",

		"Fields": [
			{ "SourcePath": "./RISKSCORE", "Targets": [{ "Name": "Commercial Delphi Score" }] },
			{ "SourcePath": "./STABILITYODDS", "Targets": [{ "Name": "Stability Odds" }] },
			{ "SourcePath": "./RISKBANDTEXT", "Targets": [{ "Name": "Commercial Delphi Band Text" }] }
		]
	},
	{
		"Name": "Limited Company Commerical Delphi Credit Limit",

		"PathToParent": "./REQUEST/DL78",

		"Fields": [
			{ "SourcePath": "./CREDITLIMIT", "Targets": [{ "Name": "Commercial Delphi Credit Limit",
				"Transformation": { "Types": [ "money" ] }
			}] }
		]
	},
	{
		"Name": "Previous Company Registered Office Address",

		"PathToParent": "./REQUEST/DL15/PREVCOMPNAMES",

		"MetaData": {
			"DisplayDirection": "horizontal"
		},

		"Fields": [
			{ "SourcePath": "./DATECHANGED", "Targets": [{ "Name": "Date Changed" }] },
			{ "SourcePath": "./PREVREGADDR1", "Targets": [{ "Name": "Office Address", "Position": 1 }] },
			{ "SourcePath": "./PREVREGADDR2", "Targets": [{ "Name": "Office Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./PREVREGADDR3", "Targets": [{ "Name": "Office Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./PREVREGADDR4", "Targets": [{ "Name": "Office Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./PREVREGPOSTCODE", "Targets": [{ "Name": "Office Address", "Position": 5, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Limited Company Details",

		"PathToParent": "./REQUEST/DL13",

		"Fields": [
			{
				"SourcePath": "./ASREGOFFICEFLAG",
				"Targets": [{
					"Name": "Same Trading Address",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Yes",
							"N": "No"
						}
					}
				}]
			},
			{ "SourcePath": "./LEN1992SIC", "Targets": [{ "Name": "Length of 1992 SIC area" }] },
			{ "SourcePath": "./TRADPHONENUM", "Targets": [{ "Name": "Trading Phone Number" }] },
			{ "SourcePath": "./PRINACTIVITIES", "Targets": [{ "Name": "Principal Activities" }] },
			{ "SourcePath": "./SIC1992DESC1", "Targets": [{ "Name": "First 1992 SIC Code Description" }] }
		]
	},
	{
		"Name": "Limited Company Bank Details",

		"PathToParent": "./REQUEST/DL17",

		"Fields": [
			{ "SourcePath": "./BANKSORTCODE", "Targets": [{ "Name": "Bank Sortcode" }] },
			{ "SourcePath": "./BANKNAME", "Targets": [{ "Name": "Bank Name" }] },

			{ "SourcePath": "./BANKADDR1", "Targets": [{ "Name": "Bank Address", "Position": 1 }] },
			{ "SourcePath": "./BANKADDR2", "Targets": [{ "Name": "Bank Address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKADDR3", "Targets": [{ "Name": "Bank Address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKADDR4", "Targets": [{ "Name": "Bank Address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./BANKPOSTCODE", "Targets": [{ "Name": "Bank Address", "Position": 5, "Prefix": "\n" }] }
		]
	},
	{
		"Name": "Limited Company Ownership Details",

		"PathToParent": "./REQUEST/DL23",

		"Fields": [
			{
				"SourcePath": "./ULTPARREGNUM",
				"Targets": [{ "Name": "Registered Number of the Current Ultimate Parent Company" }]
			},
			{
				"SourcePath": "./ULTPARNAME",
				"Targets": [{ "Name": "Registered Name of the Current Ultimate Parent Company" }]
			}
		]
	},
	{
		"Name": "Limited Company Shareholders",

		"PathToParent": "./REQUEST/DL23/SHAREHLDS",

		"MetaData": {
			"ID": "DL23SHAREHOLDING",
			"DisplayDirection": "horizontal",
			"UnlimitedWidth": true,
			"Sorting": "Description of Shareholder,Description of Shareholding,% of Shareholding"
		},

		"Fields": [
			{ "SourcePath": "./SHLDNAME", "Targets": [{ "Name": "Description of Shareholder" }] },
			{ "SourcePath": "./SHLDHOLDING", "Targets": [{ "Name": "Description of Shareholding", "Transformation": { "types": [ "shares" ] } }] }
		]
	},
	{
		"Name": "Limited Company Shareholders Details",

		"PathToParent": "./REQUEST/DLB5",

		"MetaData": {
			"DisplayDirection": "horizontal",
			"UnlimitedWidth": true,
			"ID": "DLB5"
		},

		"Fields": [
			{ "SourcePath": "./RECORDTYPE", "Targets": [{ "Name": "Record type" }] },
			{ "SourcePath": "./ISSUINGCOMPANY", "Targets": [{ "Name": "Issue company" }] },
			{ "SourcePath": "./CURPREVFLAG", "Targets": [{ "Name": "Current/previous indicator" }] },
			{ "SourcePath": "./EFFECTIVEDATE-YYYY", "Targets": [{ "Name": "Effective Date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./EFFECTIVEDATE-MM",   "Targets": [{ "Name": "Effective Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./EFFECTIVEDATE-DD",   "Targets": [{ "Name": "Effective Date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./SHARECLASSNUM", "Targets": [{ "Name": "Share class number" }] },
			{ "SourcePath": "./SHAREHOLDINGNUM", "Targets": [{ "Name": "Shareholding number" }] },
			{ "SourcePath": "./SHAREHOLDERNUM", "Targets": [{ "Name": "Shareholder number" }] },
			{ "SourcePath": "./SHAREHOLDERTYPE", "Targets": [{ "Name": "Shareholder type" }] },
			{ "SourcePath": "./NAMEPREFIX", "Targets": [{ "Name": "Shareholder Name" }] },
			{ "SourcePath": "./FIRSTNAME",  "Targets": [{ "Name": "Shareholder Name", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./MIDNAME",    "Targets": [{ "Name": "Shareholder Name", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./SURNAME",    "Targets": [{ "Name": "Shareholder Name", "Position": 3, "Prefix": " " }] },
			{ "SourcePath": "./NAMESUFFIX", "Targets": [{ "Name": "Shareholder Name", "Position": 4, "Prefix": " " }] },
			{ "SourcePath": "./QUAL", "Targets": [{ "Name": "Shareholder qualifications" }] },
			{ "SourcePath": "./TITLE", "Targets": [{ "Name": "Shareholder title" }] },
			{ "SourcePath": "./COMPANYNAME", "Targets": [{ "Name": "Shareholder company name" }] },
			{ "SourcePath": "./SHAREHOLDERKGEN", "Targets": [{ "Name": "Kgen name" }] },
			{ "SourcePath": "./SHAREHOLDERREGNUM", "Targets": [{ "Name": "Shareholder registered number" }] },
			{ "SourcePath": "./ADDRLINE1", "Targets": [{ "Name": "Shareholder address" }] },
			{ "SourcePath": "./ADDRLINE2", "Targets": [{ "Name": "Shareholder address", "Position": 1, "Prefix": "\n" }] },
			{ "SourcePath": "./ADDRLINE3", "Targets": [{ "Name": "Shareholder address", "Position": 2, "Prefix": "\n" }] },
			{ "SourcePath": "./TOWN",      "Targets": [{ "Name": "Shareholder address", "Position": 3, "Prefix": "\n" }] },
			{ "SourcePath": "./COUNTY",    "Targets": [{ "Name": "Shareholder address", "Position": 4, "Prefix": "\n" }] },
			{ "SourcePath": "./POSTCODE",  "Targets": [{ "Name": "Shareholder address", "Position": 5, "Prefix": "\n" }] },
			{ "SourcePath": "./COUNTRYOFORIGIN", "Targets": [{ "Name": "Shareholder country" }] },
			{ "SourcePath": "./PUNAPOSTCODE", "Targets": [{ "Name": "Shareholder puna pcode " }] },
			{ "SourcePath": "./RMC", "Targets": [{ "Name": "Shareholder RMC" }] },
			{ "SourcePath": "./SUPPRESS", "Targets": [{ "Name": "Suppression flag" }] },
			{ "SourcePath": "./NOCREF", "Targets": [{ "Name": "NOC ref number" }] },
			{ "SourcePath": "./LASTUPDATEDDATE-YYYY", "Targets": [{ "Name": "Last Updated", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./LASTUPDATEDDATE-MM",   "Targets": [{ "Name": "Last Updated", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./LASTUPDATEDDATE-DD",   "Targets": [{ "Name": "Last Updated", "Position": 1, "Prefix": " " }] }
		]
	},
	{
		"Name": "Limited Company Current Directorship Details",

		"MetaData": {
			"DisplayDirection": "horizontal"
		},

		"PathToParent": "./REQUEST/DL72",

		"Fields": [
			{ "SourcePath": "./FOREIGNFLAG", "Targets": [
				{ "Name": "Foreign Address Flag" },
				{
					"Name": "Foreign Address Flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Foreign",
							"N": "Not foreign"
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
			{ "SourcePath": "./DIRNUMBER", "Targets": [{ "Name": "Director number" }] },
			{ "SourcePath": "./DIRSHIPLEN", "Targets": [{
				"Name": "Length of directorship",
				"Transformation": {
					"Types": [ "monthsandyears" ]
				}
			}] },
			{ "SourcePath": "./DIRAGE", "Targets": [{ "Name": "Director�s age (Years)" }] },
			{ "SourcePath": "./NUMCONVICTIONS", "Targets": [{ "Name": "Number of convictions" }] },
			{ "SourcePath": "./DIRNAMEPREFIX", "Targets": [{ "Name": "Name" }] },
			{ "SourcePath": "./DIRFORENAME", "Targets": [{ "Name": "Name", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./DIRMIDNAME1", "Targets": [{ "Name": "Name", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DIRMIDNAME2", "Targets": [{ "Name": "Name", "Position": 3, "Prefix": " " }] },
			{ "SourcePath": "./DIRSURNAME", "Targets": [{ "Name": "Name", "Position": 4, "Prefix": " " }] },
			{ "SourcePath": "./DIRNAMESUFFIX", "Targets": [{ "Name": "Name", "Position": 5, "Prefix": " " }] },
			{ "SourcePath": "./DIRQUALS", "Targets": [{ "Name": "Qualifications" }] },
			{ "SourcePath": "./DIRTITLE", "Targets": [{ "Name": "Director title" }] },
			{ "SourcePath": "./DIRCOMPNAME", "Targets": [{ "Name": "Company name" }] },
			{ "SourcePath": "./DIRCOMPNUM", "Targets": [{ "Name": "Company number" }] }
		]
	},
	{
		"Name": "Limited Company Directorship Summary",

		"PathToParent": "./REQUEST/DL42",

		"Fields": [
			{ "SourcePath": "./TOTCURRDIRS", "Targets": [{ "Name": "Total Number of Current Directors" }] },
			{ "SourcePath": "./CURRDIRSHIPSLAST12", "Targets": [{ "Name": "Number of Current Directorships Less Than 12 Months" }] },
			{ "SourcePath": "./APPTSLAST12", "Targets": [{ "Name": "Number of Appointments in the Last 12 Months" }] },
			{ "SourcePath": "./RESNSLAST12", "Targets": [{ "Name": "Number of Resignations in the Last 12 Months" }] }
		]
	},
	{
		"Name": "Limited Company CCJ Summary",

		"PathToParent": "./REQUEST/DL26",

		"Fields": [
			{ "SourcePath": "./AGEMOSTRECENTCCJ", "Targets": [{ "Name": "Age of Most Recent CCJ/Decree (Months)" }] },
			{ "SourcePath": "./NUMCCJLAST12", "Targets": [{ "Name": "Number of CCJs During Last 12 Months" }] },
			{ "SourcePath": "./VALCCJLAST12", "Targets": [{ "Name": "Value of CCJs During Last 12 Months" }] },
			{ "SourcePath": "./NUMCCJ13TO24", "Targets": [{ "Name": "Number of CCJs Between 13 And 24 Months Ago" }] },
			{ "SourcePath": "./VALCCJ13TO24", "Targets": [{ "Name": "Value of CCJs Between 13 And 24 Months Ago" }] }
		]
	},
	{
		"Name": "Limited Company Credit Summary",

		"PathToParent": "./REQUEST/DL27/SUMMARYLINE",

		"Fields": [
			{
				"SourcePath": "./CREDTYPE",
				"Targets": [
					{ "Name": "Credit Event Type" },
					{
						"Name": "Credit Event Type",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"C": "Receiver appointments",
								"D": "Cessations of Receiver",
								"E": "Winding up petitions",
								"F": "Dismissals of winding up petitions",
								"G": "Winding up orders",
								"H": "Voluntary appointments of liquidators",
								"I": "Meetings of creditors",
								"J": "Resolutions to wind up",
								"K": "Intentions to dissolve",
								"L": "Dissolution notices",
								"M": "Reinstatement notices",
								"Q": "Administrators appointed",
								"R": "Administrators dismissals",
								"S": "Approvals of Voluntary arrangements",
								"T": "Completions of Voluntary arrangements",
								"U": "Compulsory appointments of liquidators",
								"V": "Revocations of Voluntary arrangements",
								"W": "Suspensions of Voluntary arrangements"
							}
						}
					}
				]
			},
			{
				"SourcePath": "./TYPEDATE-YYYY", "Targets": [{ "Name": "Date of Most Recent Record for Type", "Position": 2, "Prefix": " " }]
			},
			{
				"SourcePath": "./TYPEDATE-MM",
				"Targets": [{
					"Name": "Date of Most Recent Record for Type",
					"Position": 0,
					"Transformation": {
						"Types": [ "monthname" ]
					}
				}]
			},
			{
				"SourcePath": "./TYPEDATE-DD", "Targets": [{ "Name": "Date of Most Recent Record for Type", "Position": 1, "Prefix": " " }]
			}
		]
	},
	{
		"Name": "Limited Company Payment Performance Details",

		"PathToParent": "./REQUEST/DL41",

		"MetaData": {
			"ID": "DL41"
		},

		"Fields": [
			{ "SourcePath": "./COMPAVGDBT-3MTH", "Targets": [{ "Name": "Company - Average DBT - 3 Months" }] },
			{ "SourcePath": "./COMPAVGDBT-6MTH", "Targets": [{ "Name": "Company - Average DBT - 6 Months" }] },
			{ "SourcePath": "./COMPAVGDBT-12MTH", "Targets": [{ "Name": "Company - Average DBT - 12 Months" }] },
			{ "SourcePath": "./COMPNUMDBT-1000", "Targets": [{ "Name": "Company - Number of DBT (?0-?1,000)" }] },
			{ "SourcePath": "./COMPNUMDBT-10000", "Targets": [{ "Name": "Company - Number of DBT (?1,000-?10,000)" }] },
			{ "SourcePath": "./COMPNUMDBT-100000", "Targets": [{ "Name": "Company - Number of DBT (?10,000-?100,000)" }] },
			{ "SourcePath": "./COMPNUMDBT-100000PLUS", "Targets": [{ "Name": "Company - Number of DBT (?100,000+)" }] },
			{ "SourcePath": "./INDAVGDBT-3MTH", "Targets": [{ "Name": "Industry - Average DBT - 3 Months" }] },
			{ "SourcePath": "./INDAVGDBT-6MTH", "Targets": [{ "Name": "Industry - Average DBT - 6 Months" }] },
			{ "SourcePath": "./INDAVGDBT-12MTH", "Targets": [{ "Name": "Industry - Average DBT - 12 Months" }] },
			{ "SourcePath": "./INDNUMDBT-1000", "Targets": [{ "Name": "Industry - Number of DBT (?0-?1,000)" }] },
			{ "SourcePath": "./INDNUMDBT-10000", "Targets": [{ "Name": "Industry - Number of DBT (?1,000-?10,000)" }] },
			{ "SourcePath": "./INDNUMDBT-100000", "Targets": [{ "Name": "Industry - Number of DBT (?10,000-?100,000)" }] },
			{ "SourcePath": "./INDNUMDBT-100000PLUS", "Targets": [{ "Name": "Industry - Number of DBT (?100,000+)" }] },
			{ "SourcePath": "./COMPPAYPATTN", "Targets": [
				{ "Name": "Company Payment Pattern" },
				{
					"Name": "Company Payment Pattern",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"C": "Consistent",
							"W": "Worsening",
							"N": "Noticeable Worsening",
							"S": "Significant Worsening",
							"I": "Improvement",
							"O": "Noticeable Improvement",
							"T": "Significant Improvement"
						}
					}
				}
			]},
			{ "SourcePath": "./INDPAYPATTN", "Targets": [
				{ "Name": "Industry Payment Pattern" },
				{
					"Name": "Industry Payment Pattern",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"C": "Consistent",
							"S": "Slower",
							"F": "Faster"
						}
					}
				}
			]},
			{ "SourcePath": "./SUPPPAYPATTN", "Targets": [
				{ "Name": "Supplier Payment Pattern" },
				{
					"Name": "Supplier Payment Pattern",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"N": "No or Little Difference",
							"S": "Slower",
							"F": "Faster"
						}
					}
				}
			]}
		]
	},
	{
		"Name": "Limited Company CIFAS Details",

		"PathToParent": "./REQUEST/DL48",

		"Fields": [
			{
				"SourcePath": "./FRAUDCATEGORY",
				"Targets": [
					{ "Name": "Fraud Category" },
					{
						"Name": "Fraud Category",
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
				]
			},
			{ "SourcePath": "./SUPPLIERNAME", "Targets": [{ "Name": "Supplier Name" }] }
		]
	},
	{
		"Name": "Limited Company 652/3 Notices",

		"PathToParent": "./REQUEST/DL52",

		"Fields": [
			{
				"SourcePath": "./RECORDTYPE",
				"Targets": [
					{ "Name": "Notice Type" },
					{
						"Name": "Notice Type",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"K": "Intention to dissolve",
								"L": "Company dissolved",
								"M": "Company reinstated"
							}
						}
					}
				]
			},
			{ "SourcePath": "./DATEOFNOTICE-YYYY", "Targets": [{ "Name": "Date of Notice", "Position": 2, "Prefix": " " }] },
			{
				"SourcePath": "./DATEOFNOTICE-MM",
				"Targets": [{
					"Name": "Date of Notice",
					"Position": 0,
					"Transformation": {
						"Types": [ "monthname" ]
					}
				}]
			},
			{ "SourcePath": "./DATEOFNOTICE-DD", "Targets": [{ "Name": "Date of Notice", "Position": 1, "Prefix": " " }] }
		]
	},
	{
		"Name": "Limited Company UK Subsidiaries",

		"PathToParent": "./REQUEST/DL68",

		"Fields": [
			{ "SourcePath": "./SUBSIDREGNUM", "Targets": [{ "Name": "Subsidiary registered number" }] },
			{
				"SourcePath": "./SUBSIDSTATUS",
				"Targets": [
					{ "Name": "Subsidiary status" },
					{
						"Name": "Subsidiary status",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"L": "Live",
								"D": "Dormant",
								"S": "Dissolved"
							}
						}
					}
				]
			},
			{
				"SourcePath": "./SUBSIDLEGALSTATUS",
				"Targets": [
					{ "Name": "Subsidiary legal status" },
					{
						"Name": "Subsidiary legal status",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"1": "Private Unlimited",
								"2": "Private Limited",
								"3": "PLC",
								"4": "Old Public Company",
								"5": "Private Company Limited by Guarantee (Exempt from using word Limited)",
								"6": "Limited Partnership",
								"7": "Private Limited Company Without Share Capital",
								"8": "Company Converted / Closed",
								"9": "Private Unlimited Company Without Share Capital",
								"0": "Other",
								"A": "Private Company Limited by Shares (Exempt from using word Limited)"
							}
						}
					}
				]
			},
			{ "SourcePath": "./SUBSIDNAME", "Targets": [{ "Name": "Subsidiary name" }] }
		]
	},
	{
		"Name": "Limited Company Instalment CAIS Details",

		"PathToParent": "./REQUEST/DL97",

		"Fields": [
			{
				"SourcePath": "./ACCTSTATE",
				"Targets": [
					{ "Name": "Account State" },
					{
						"Name": "Account State",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"A": "Active",
								"D": "Defaulted",
								"S": "Settled"
							}
						}
					}
				]
			},
			{
				"SourcePath": "./COMPANYTYPE",
				"Targets": [
					{ "Name": "Company Type" },
					{
						"Name": "Company Type",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"1": "Bank",
								"2": "Finance House",
								"3": "Retailer",
								"4": "Mail Order",
								"5": "Collection Agency",
								"6": "TV Rental",
								"7": "Insurance",
								"8": "Building Society",
								"9": "Credit Card"
							}
						}
					}
				]
			},
			{
				"SourcePath": "./ACCTTYPE",
				"Targets": [
					{ "Name": "Account Type" },
					{
						"Name": "Account Type",
						"Position": 1,
						"Prefix": " - ",
						"Transformation": {
							"Types": [ "map" ],
							"Map": {
								"1":  "Hire Purchase",
								"2":  "Loan",
								"3":  "Mortgage",
								"4":  "Budget Account",
								"5":  "Credit Card",
								"6":  "Charge Card",
								"7":  "Rental",
								"8":  "Mail Order Agency",
								"9":  "Mail Order Direct",
								"10": "Mail Order Cash",
								"11": "Overdraft",
								"15": "Current Account",
								"16": "Secured Loan",
								"17": "Credit Sale",
								"18": "Conditional Sale",
								"19": "Primary Lease",
								"20": "Secondary Lease",
								"21": "Dealer Buy-Back",
								"22": "Balloon Rental"
							}
						}
					}
				]
			},
			{ "SourcePath": "./DEFAULTDATE-YYYY", "Targets": [{ "Name": "Default Date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DEFAULTDATE-MM",   "Targets": [{ "Name": "Default Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DEFAULTDATE-DD",   "Targets": [{ "Name": "Default Date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./SETTLEMTDATE-YYYY", "Targets": [{ "Name": "Settlement Date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./SETTLEMTDATE-MM",   "Targets": [{ "Name": "Settlement Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./SETTLEMTDATE-DD",   "Targets": [{ "Name": "Settlement Date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./DEFAULTBALANCE", "Targets": [{ "Name": "Default Balance" }] },
			{ "SourcePath": "./CURRBALANCE", "Targets": [{ "Name": "Current Balance" }] },
			{ "SourcePath": "./STATUS1TO2", "Targets": [{ "Name": "Status 1-2" }] },
			{ "SourcePath": "./STATUS3TO9", "Targets": [{ "Name": "Status 3-9" }] },
			{ "SourcePath": "./CAISLASTUPDATED-YYYY", "Targets": [{ "Name": "CAIS Last Updated Date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./CAISLASTUPDATED-MM",   "Targets": [{ "Name": "CAIS Last Updated Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./CAISLASTUPDATED-DD",   "Targets": [{ "Name": "CAIS Last Updated Date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./ACCTSTATUS12", "Targets": [{ "Name": "Account status (Last 12 Account Statuses" }] },
			{ "SourcePath": "./AGREEMTNUM", "Targets": [{ "Name": "Agreement Number" }] },
			{ "SourcePath": "./MONTHSDATA", "Targets": [{ "Name": "Months Data" }] }
		]
	},
	{
		"Name": "Limited Company Financial Details IFRS & UK GAAP",

		"PathToParent": "./REQUEST/DL99",

		"MetaData": {
			"ID": "DL99",
			"Sorting": "chart-Total Current Assets,chart-Net Assets,chart-Bank Overdraft,chart-Other Short-Term Loans,chart-Total Current Liabilities,chart-Other Long-Term Loans,chart-Total Shareholders Funds"
		},

		"Fields": [
			{ "SourcePath": "./TOTALCURRASSETS", "Targets": [{ "Name": "chart-Total Current Assets", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./NETASSETS", "Targets": [{ "Name": "chart-Net Assets", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./TOTALSHAREFUND", "Targets": [{ "Name": "chart-Total Shareholders Funds", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./BANKOVERDRAFT", "Targets": [{ "Name": "chart-Bank Overdraft", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./FINOTHLOANS", "Targets": [{ "Name": "chart-Other Short-Term Loans", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./TOTALCURRLBLTS", "Targets": [{ "Name": "chart-Total Current Liabilities", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./FINLBLTSOTHLOANS", "Targets": [{ "Name": "chart-Other Long-Term Loans", "Transformation": { "Types": [ "money" ] } }] },
			{ "SourcePath": "./DATEOFACCOUNTS-YYYY", "Targets": [{ "Name": "ticks-Accounts Date", "Position": 2, "Prefix": " " }] },
			{ "SourcePath": "./DATEOFACCOUNTS-MM",   "Targets": [{ "Name": "ticks-Accounts Date", "Position": 0, "Transformation": { "Types": [ "monthname" ] } }] },
			{ "SourcePath": "./DATEOFACCOUNTS-DD",   "Targets": [{ "Name": "ticks-Accounts Date", "Position": 1, "Prefix": " " }] },
			{ "SourcePath": "./CURRENCY", "Targets": [{ "Name": "Currency" }] }
		]
	},
	{
		"Name": "Limited Company Mortages",

		"PathToParent": "./REQUEST/DL65",

		"MetaData": {
			"DisplayDirection": "horizontal",
			"UnlimitedWidth": true,
			"Sorting": "Alterations to the order,Total Amount of Debenture Secured,*"
		},

		"Fields": [
			{ "SourcePath": "./CHARGENUMBER", "Targets": [{ "Name": "Charge Number" }] },
			{ "SourcePath": "./FORMNUMBERFLAG", "Targets": [
				{ "Name": "Form Number" },
				{ "Name": "Form Number", "Position": 1, "Prefix": " - ", "Transformation": {
					"Types": [ "map" ],
					"Map": {
						"04": "�A� � Mortgage extracted from credit master file: All charges until autumn 1994",
						"08": "�B� � Satisfaction of a mortgage originally extracted from the credit master file as record type �A�",
						"12": "�9999� � Correction to charge details",
						"16": "�395� � Mortgage or charge registration",
						"20": "�400� � Mortgage or charge subject to which property has been acquired",
						"24": "�397� � Charge securing a series of debentures",
						"28": "�397A� � Issue of secured debentures in a series",
						"32": "�403A� � Satisfaction in full or part of a charge",
						"36": "�403B� � Part of property or undertaking charged has been released from the charge or no longer part of the company�s property/undertaking",
						"40": "�4051� � Appointment of receiver or manager",
						"44": "�4052� � Cessation of receiver or manager",
						"48": "�410� � Scottish mortgage/charge registration",
						"52": "�413� � Charge securing a series of debentures",
						"56": "�413A� � Issue of secured debentures in a series",
						"60": "�416� � Mortgage or charge subject to which property has been acquired",
						"64": "�419A� � Satisfaction in full or part of a mortgage or charge",
						"68": "�419B� � Declaration that part of the property or undertaking has been released from the charge or no longer forms part of the company�s property or undertaking",
						"72": "�1SC� � Appointment of a receiver by the holder of a floating charge",
						"76": "�2SC� � Appointment of a receiver by the court",
						"80": "�3SC� � Cessation of a receiver",
						"84": "�33SC� � Death of a receiver",
						"88": "�466� � Instrument of alteration to a floating charge"
					}
				}}
			]},
			{ "SourcePath": "./CURRENCYINDICATOR", "Targets": [{ "Name": "Currency Indicator" }] },
			{ "SourcePath": "./TOTAMTDEBENTURESECD", "Targets": [{ "Name": "Total Amount of Debenture Secured" }] },
			{ "SourcePath": "./CHARGETYPE", "Targets": [{ "Name": "Charge type" }] },
			{ "SourcePath": "./AMTSECURED", "Targets": [{ "Name": "Amount secured" }] },
			{ "SourcePath": "./PROPERTYDETAILS", "Targets": [{ "Name": "Property details" }] },
			{ "SourcePath": "./CHARGEETEXT", "Targets": [{ "Name": "Chargee text" }] },
			{ "SourcePath": "./RESTRICTINGPROVNS", "Targets": [{ "Name": "Restricting provisions" }] },
			{ "SourcePath": "./REGULATINGPROVNS", "Targets": [{ "Name": "Regulating provisions" }] },
			{ "SourcePath": "./ALTERATIONSTOORDER", "Targets": [{ "Name": "Alterations to the order" }] },
			{ "SourcePath": "./PROPERTYRELDFROMCHGE", "Targets": [{ "Name": "Property released from the charge" }] },
			{ "SourcePath": "./AMTCHARGEINCRD", "Targets": [{ "Name": "Amount charge increased" }] }
		]
	}
]','Experian parser configuration for Company Score tab in Underwriter')
GO

INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('DirectorInfoParserConfiguration','[
	{
		"Name": "Limited Company Current Directorship Details",

		"PathToParent": "./REQUEST/DL72",

		"Fields": [
			{ "SourcePath": "./FOREIGNFLAG", "Targets": [
				{ "Name": "Foreign Address Flag" },
				{ "Name": "Foreign Address Flag",
					"Position": 1,
					"Prefix": " - ",
					"Transformation": {
						"Types": [ "map" ],
						"Map": {
							"Y": "Foreign",
							"N": "Not foreign"
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
			{ "SourcePath": "./DIRNUMBER", "Targets": [{ "Name": "Director number" }] },
			{ "SourcePath": "./DIRSHIPLEN", "Targets": [{
				"Name": "Length of directorship",
				"Transformation": {
					"Types": [ "monthsandyears" ]
				}
			}] },
			{ "SourcePath": "./DIRAGE", "Targets": [{ "Name": "Director�s age (Years)" }] },
			{ "SourcePath": "./NUMCONVICTIONS", "Targets": [{ "Name": "Number of convictions" }] },
			{ "SourcePath": "./DIRNAMEPREFIX", "Targets": [{ "Name": "NamePrefix" }] },
			{ "SourcePath": "./DIRFORENAME", "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./DIRMIDNAME1", "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./DIRMIDNAME2", "Targets": [{ "Name": "MidName2" }] },
			{ "SourcePath": "./DIRSURNAME", "Targets": [{ "Name": "LastName" }] },
			{ "SourcePath": "./DIRNAMESUFFIX", "Targets": [{ "Name": "NameSuffix" }] },
			{ "SourcePath": "./DIRQUALS", "Targets": [{ "Name": "Qualifications" }] },
			{ "SourcePath": "./DIRTITLE", "Targets": [{ "Name": "Director title" }] },
			{ "SourcePath": "./DIRCOMPNAME", "Targets": [{ "Name": "Company name" }] },
			{ "SourcePath": "./DIRCOMPNUM", "Targets": [{ "Name": "Company number" }] }
		]
	}
]
','Experian parser configuration for extracting director information') GO
