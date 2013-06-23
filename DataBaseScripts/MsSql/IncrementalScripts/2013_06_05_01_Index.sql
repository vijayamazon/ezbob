IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LoanSchedule]') AND name = N'IX_LoanSchedule_LoanId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId]
	ON [dbo].[LoanSchedule] ([Status])
	INCLUDE ([Date],[LoanId])
END
GO


