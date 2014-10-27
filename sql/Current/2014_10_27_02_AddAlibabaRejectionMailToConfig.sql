

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailCc')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailCc', 'dev-ops@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailTo')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailTo', 'dev-alibaba@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailCc')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailCc', 'qa-ops@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailTo')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailTo', 'qa-alibaba@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailCc')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailCc', 'ops@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailTo')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailTo', 'alibaba@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
	END
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaCurrencyConversionCoefficient')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaCurrencyConversionCoefficient', '1.05', 'Coefficiend for conversion of GBP to USD')
END
GO

