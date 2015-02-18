IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE name = 'AutoApproveTurnoverDropQuarterRatio')
BEGIN
	INSERT INTO ConfigurationVariables (Name,Value,Description,IsEncrypted) VALUES (
		'AutoApproveTurnoverDropQuarterRatio','0.75','Last quarter turnover (annualized) marketplace should be at least this percent of annual turnover.',0
	)
END
GO
