IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'LimitedMedalDaysOfMpRelevancy')
	UPDATE ConfigurationVariables SET Name = 'MedalDaysOfMpRelevancy' WHERE Name = 'LimitedMedalDaysOfMpRelevancy'
	
IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'LimitedMedalMinOffer')
	UPDATE ConfigurationVariables SET Name = 'MedalMinOffer' WHERE Name = 'LimitedMedalMinOffer'
GO
	