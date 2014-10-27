IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailTo')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailTo', 'alibaba@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaMailCc')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaMailCc', 'ops@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected or approved')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaCurrencyConversionCoefficient')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaCurrencyConversionCoefficient', '1.05', 'Coefficiend for conversion of GBP to USD')
END
GO

