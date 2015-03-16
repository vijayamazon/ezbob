UPDATE ConfigurationVariables SET Value = '[
	{
		"Name": "Directors",

		"PathToParent": "./REQUEST/DD11",

		"Fields": [
			{ "SourcePath": "./DIRCOMPFLAG", "Targets": [{ "Name": "IsCompany" }] },

			{ "SourcePath": "./NAMEPREFIX",  "Targets": [{ "Name": "Prefix" }] },
			{ "SourcePath": "./FORENAME", "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./MIDNAME1", "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./MIDNAME2", "Targets": [{ "Name": "MidName2" }] },
			{ "SourcePath": "./SURNAME", "Targets": [{ "Name": "LastName" }] },
			{ "SourcePath": "./DIRTITLE",  "Targets": [{ "Name": "Title" }] },
			{ "SourcePath": "./HOUSENAME", "Targets": [{ "Name": "HouseName" }] },
			{ "SourcePath": "./HOUSENUM", "Targets": [{ "Name": "HouseNumber" }] },
			{ "SourcePath": "./STREET", "Targets": [{ "Name": "Street" }] },
			{ "SourcePath": "./TOWN", "Targets": [{ "Name": "Town" }] },
			{ "SourcePath": "./COUNTY", "Targets": [{ "Name": "County" }] },
			{ "SourcePath": "./POSTCODE", "Targets": [{ "Name": "Postcode" }] }
		]
	}
]'
WHERE Name = 'DirectorDetailsNonLimitedParserConfiguration'
GO
