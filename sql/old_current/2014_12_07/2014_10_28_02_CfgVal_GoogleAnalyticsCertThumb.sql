IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'GoogleAnalyticsCertThumb')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'GoogleAnalyticsCertThumb',
		'08 a1 90 d7 e7 b6 1e 5c df a6 33 01 e5 28 13 4d 36 99 f0 96',
		'CertThumb for downloading Google Analytics data.',
		0
	)
END
GO
