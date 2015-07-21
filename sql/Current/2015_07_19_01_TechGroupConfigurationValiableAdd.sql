IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='EzbobTechMailTo')
BEGIN
	INSERT INTO dbo.ConfigurationVariables(Name,[Value],Description) VALUES('EzbobTechMailTo', 'tech@ezbob.com', 'Mail group for technical team')
END
GO



