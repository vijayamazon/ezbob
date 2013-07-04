IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='EnableAutomaticReRejection')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('EnableAutomaticReRejection','0','if Enabled system will Re-Reject customers automatically without any Underwriter actions')
END
GO


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='AutoRejectionException_CreditScore')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('AutoRejectionException_CreditScore','900','Experian Consumer score that will prevent customer from automatic rejection')
END
GO


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='AutoRejectionException_AnualTurnover')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('AutoRejectionException_AnualTurnover','250000','Anual Turnover that will prevent customer from automatic rejection')
END
GO
