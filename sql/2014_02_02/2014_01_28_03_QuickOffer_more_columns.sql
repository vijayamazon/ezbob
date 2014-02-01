IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('QuickOffer') AND name = 'ImmediateTerm')
BEGIN
	ALTER TABLE QuickOffer ADD ImmediateTerm INT NULL
	ALTER TABLE QuickOffer ADD ImmediateInterestRate DECIMAL(18, 6) NULL
	ALTER TABLE QuickOffer ADD ImmediateSetupFee DECIMAL(18, 6) NULL
	ALTER TABLE QuickOffer ADD PotentialAmount DECIMAL(18, 6) NULL
	ALTER TABLE QuickOffer ADD PotentialTerm INT NULL
	ALTER TABLE QuickOffer ADD PotentialInterestRate DECIMAL(18, 6) NULL
	ALTER TABLE QuickOffer ADD PotentialSetupFee DECIMAL(18, 6) NULL
END
GO
