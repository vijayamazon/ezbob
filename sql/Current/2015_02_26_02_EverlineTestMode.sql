DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineLoanStatusTestMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineLoanStatusTestMode', '', 'empty / DoesNotExist / ExistsWithNoLiveLoan / ExistsWithCurrentLiveLoan')
	END
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineLoanStatusTestMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineLoanStatusTestMode', '', 'empty / DoesNotExist / ExistsWithNoLiveLoan / ExistsWithCurrentLiveLoan')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineLoanStatusTestMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineLoanStatusTestMode', '', 'empty / DoesNotExist / ExistsWithNoLiveLoan / ExistsWithCurrentLiveLoan')
	END
END
GO