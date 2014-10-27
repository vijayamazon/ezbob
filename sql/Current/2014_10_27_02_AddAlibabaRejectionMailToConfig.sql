IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaRejectionMailTo')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaRejectionMailTo', 'ops@ezbob.com', 'An email to this address will be sent when Alibaba customer is rejected')
END
GO
