IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'SetupFeeFixed')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('SetupFeeFixed', '30', 'Fixed setup fee (int)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'SetupFeePercent')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('SetupFeePercent', '0.8', 'Percent setup fee')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'SetupFeeMaxFixedPercent')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('SetupFeeMaxFixedPercent', 'True', 'Determines if the max or min between the two should be used')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'SetupFeeEnabled')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('SetupFeeEnabled', 'False', 'Determines if the setup fee is wnabled by default')
END
GO

