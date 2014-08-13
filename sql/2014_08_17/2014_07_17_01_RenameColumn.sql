IF OBJECT_ID('ExperianNonLimitedResults') IS NOT NULL
BEGIN
	IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TotalJudgmentCountValue24Months' and Object_ID = Object_ID(N'ExperianNonLimitedResults'))
	BEGIN
		EXEC sp_rename 'ExperianNonLimitedResults.TotalJudgmentCountValue24Months', 'TotalJudgmentValueLast24Months', 'COLUMN'
	END
END
GO
