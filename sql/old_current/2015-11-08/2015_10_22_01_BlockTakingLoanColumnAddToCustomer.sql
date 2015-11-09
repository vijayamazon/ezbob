
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='BlockTakingLoan' AND id=object_id('Customer'))
BEGIN
	ALTER TABLE Customer ADD BlockTakingLoan BIT NOT NULL DEFAULT (0)
END 
GO