SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('ExperianNonLimitedResults') AND name = 'RiskText')
BEGIN
		ALTER TABLE ExperianNonLimitedResults ADD RiskText NVARCHAR(70) NULL
		ALTER TABLE ExperianNonLimitedResults ADD CreditText NVARCHAR(560) NULL
		ALTER TABLE ExperianNonLimitedResults ADD ConcludingText NVARCHAR(200) NULL
		ALTER TABLE ExperianNonLimitedResults ADD NocText NVARCHAR(200) NULL
		ALTER TABLE ExperianNonLimitedResults ADD PossiblyRelatedDataText NVARCHAR(200) NULL
END
GO

 