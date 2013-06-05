IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LoanSchedule]') AND name = N'IX_LoanSchedule_LoanId')
RETURN
GO


CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId]
ON [dbo].[LoanSchedule] ([Status])
INCLUDE ([Date],[LoanId])
GO