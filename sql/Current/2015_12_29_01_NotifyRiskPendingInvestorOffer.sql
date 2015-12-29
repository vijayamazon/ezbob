DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PendingInvestorNoficationReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PendingInvestorNoficationReciever', '', 'email that notifies risk when investor is not found for open platform offer')
	END
END

IF @Environment = 'QA'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PendingInvestorNoficationReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PendingInvestorNoficationReciever', 'qa@ezbob.com', 'email that notifies risk when investor is not found for open platform offer')
	END
END

IF @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PendingInvestorNoficationReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PendingInvestorNoficationReciever', 'qa@ezbob.com', 'email that notifies risk when investor is not found for open platform offer')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PendingInvestorNoficationReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PendingInvestorNoficationReciever', 'risk@ezbob.com', 'email that notifies risk when investor is not found for open platform offer')
	END
END
GO
