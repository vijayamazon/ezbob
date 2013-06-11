IF NOT EXISTS (SELECT 1 FROM sysindexes WHERE name = 'IX_CustomerLoyaltyProgramCustomerId')
BEGIN
	CREATE INDEX IX_CustomerLoyaltyProgramCustomerId ON dbo.CustomerLoyaltyProgram (CustomerId)
END
GO
