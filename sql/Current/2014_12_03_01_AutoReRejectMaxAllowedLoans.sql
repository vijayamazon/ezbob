IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoReRejectMaxAllowedLoans')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES
		('AutoReRejectMaxAllowedLoans', '2', 'If customer has at least this number of open loans he is force rejected during Auto Re-reject', 0)
END
GO
