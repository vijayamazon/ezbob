SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanDailyStats') AND name = 'EarnedInterest')
	EXECUTE sp_rename 'LoanDailyStats.EarnedInterest', 'EarnedInterestByPeriods', 'COLUMN'
GO

ALTER TABLE LoanDailyStats DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanDailyStats') AND name = 'EarnedInterestBySomeDate')
	ALTER TABLE LoanDailyStats ADD EarnedInterestBySomeDate DECIMAL(18, 2) NULL
GO

ALTER TABLE LoanDailyStats ADD TimestampCounter ROWVERSION
GO
