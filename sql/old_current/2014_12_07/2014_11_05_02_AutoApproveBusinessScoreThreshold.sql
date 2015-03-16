IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveBusinessScoreThreshold')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveBusinessScoreThreshold', '50', 'Auto approve customers that have at least this business score.', 0
	)
END
GO
