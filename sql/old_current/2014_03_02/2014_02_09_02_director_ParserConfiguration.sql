UPDATE ConfigurationVariables SET Value = '[
	{
		"Name": "Limited Company Current Directorship Details",

		"PathToParent": "./REQUEST/DL72",

		"Fields": [
			{ "SourcePath": "./DIRFORENAME", "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./DIRMIDNAME1", "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./DIRMIDNAME2", "Targets": [{ "Name": "MidName2" }] },
			{ "SourcePath": "./DIRSURNAME", "Targets": [{ "Name": "LastName" }] }
		]
	}
]' WHERE Name = 'DirectorInfoParserConfiguration'
GO

UPDATE ConfigurationVariables SET Value = '[
	{
		"Name": "Non-Limited Individual Director Details",

		"PathToParent": "./REQUEST/DD11",

		"Fields": [
			{ "SourcePath": "./FORENAME", "Targets": [{ "Name": "FirstName" }] },
			{ "SourcePath": "./MIDNAME1", "Targets": [{ "Name": "MidName1" }] },
			{ "SourcePath": "./MIDNAME2", "Targets": [{ "Name": "MidName2" }] },
			{ "SourcePath": "./SURNAME", "Targets": [{ "Name": "LastName" }] }
		]
	}
]' WHERE Name = 'DirectorInfoNonLimitedParserConfiguration'
GO
