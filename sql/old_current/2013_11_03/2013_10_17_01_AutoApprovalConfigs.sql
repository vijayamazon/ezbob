IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EnableAutomaticReApproval')
BEGIN
	UPDATE ConfigurationVariables SET Name='EnableAutomaticReApproval', Description='if Enabled system will ReApprove customers automatically without any Underwriter actions' WHERE Name='EnableAutomaticApproval'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EnableAutomaticApproval')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EnableAutomaticApproval', 0, 'if Enabled system will Approve customers automatically without any Underwriter actions')
END
GO
