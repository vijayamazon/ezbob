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


IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='InvestorFundsUtilized90')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('InvestorFundsUtilized90', '0.9', 'percent of utilized funding investor''s balance for notification sending')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='InvestorFundsUtilized75')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('InvestorFundsUtilized75', '0.75', 'percent of utilized funding investor''s balance for notification sending')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='InvestorServicingFeePercent')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('InvestorServicingFeePercent', '0.01', 'percent of each op loan repayment to reduce from investor')
END
GO
