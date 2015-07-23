IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='SmsApprovedUserEnabled')
BEGIN 
	INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('SmsApprovedUserEnabled', 'False', 'True / False if to send approved user sms', NULL)
END 	
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='SmsApprovedUserTemplate')
BEGIN 
INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('SmsApprovedUserTemplate', '*FirstName*, you are approved for *LoanAmount* for *ValidFor* hours interest of *InterestRate*, sf *SetupFee* by *Origin*', 'supports following tags: *FirstName*, *LoanAmount*, *ValidFor*, *InterestRate*, *SetupFee*, *Origin* *OriginSite* *OriginPhone*', NULL)
END 	
GO
