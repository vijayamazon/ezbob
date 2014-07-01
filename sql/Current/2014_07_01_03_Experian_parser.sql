UPDATE ConfigurationVariables SET VALUE = '[
	{
		"Name": "Directors",

		"PathToParent": "./REQUEST/DL72",

		"Fields": [
			{ "SourcePath": "./DIRCOMPFLAG", "Targets": [{ "Name": "IsCompany" }] },
			{ "SourcePath": "./DIRSHAREINFO", "Targets": [{ "Name": "ShareInfo" }] },

			{ "SourcePath": "./DIRNAMEPREFIX",  "Targets": [{ "Name": "Prefix" }] },
			{ "SourcePath": "./DIRFORENAME", "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./DIRMIDNAME1", "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./DIRMIDNAME2", "Targets": [{ "Name": "MidName2" }] },
			{ "SourcePath": "./DIRSURNAME", "Targets": [{ "Name": "LastName" }] },
			{ "SourcePath": "./DIRNAMESUFFIX",  "Targets": [{ "Name": "Suffix" }] },
			{ "SourcePath": "./DIRTITLE",  "Targets": [{ "Name": "Title" }] },

			{ "SourcePath": "./DIRNUMBER", "Targets": [{ "Name": "Number" }] },

			{ "SourcePath": "./DATEOFBIRTH-YYYY", "Targets": [{ "Name": "BirthDate", "Position": 2, "Prefix": "/" }] },
			{ "SourcePath": "./DATEOFBIRTH-MM", "Targets": [{ "Name": "BirthDate", "Position": 1, "Prefix": "/"  }] },
			{ "SourcePath": "./DATEOFBIRTH-DD", "Targets": [{ "Name": "BirthDate", "Position": 0 }] },

			{ "SourcePath": "./DIRHOUSENAME", "Targets": [{ "Name": "HouseName" }] },
			{ "SourcePath": "./DIRHOUSENUM", "Targets": [{ "Name": "HouseNumber" }] },
			{ "SourcePath": "./DIRSTREET", "Targets": [{ "Name": "Street" }] },
			{ "SourcePath": "./DIRTOWN", "Targets": [{ "Name": "Town" }] },
			{ "SourcePath": "./DIRCOUNTY", "Targets": [{ "Name": "County" }] },
			{ "SourcePath": "./DIRPOSTCODE", "Targets": [{ "Name": "Postcode" }] }
		]
	},
	{
		"Name": "Shareholders",

		"PathToParent": "./REQUEST/DLB5",

		"Fields": [
			{ "SourcePath": "./NAMEPREFIX",  "Targets": [{ "Name": "Prefix" }] },
			{ "SourcePath": "./FIRSTNAME",  "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./MIDNAME",    "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./SURNAME",    "Targets": [{ "Name": "LastName" }] },
			{ "SourcePath": "./NAMESUFFIX",  "Targets": [{ "Name": "Suffix" }] },
			{ "SourcePath": "./TITLE",  "Targets": [{ "Name": "Title" }] },

			{ "SourcePath": "./ADDRLINE1", "Targets": [{ "Name": "AddressLine1" }] },
			{ "SourcePath": "./ADDRLINE2", "Targets": [{ "Name": "AddressLine2" }] },
			{ "SourcePath": "./ADDRLINE3", "Targets": [{ "Name": "AddressLine3" }] },
			{ "SourcePath": "./TOWN",      "Targets": [{ "Name": "Town" }] },
			{ "SourcePath": "./COUNTY",    "Targets": [{ "Name": "County" }] },
			{ "SourcePath": "./POSTCODE",  "Targets": [{ "Name": "Postcode" }] },
			{ "SourcePath": "./COUNTRYOFORIGIN", "Targets": [{ "Name": "Country" }] }
		]
	}
]'
WHERE Name = 'DirectorDetailsParserConfiguration'
GO
