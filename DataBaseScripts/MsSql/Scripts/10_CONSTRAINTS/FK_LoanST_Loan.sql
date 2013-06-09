IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanST_Loan]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanScheduleTransaction]'))
ALTER TABLE [dbo].[LoanScheduleTransaction] DROP CONSTRAINT [FK_LoanST_Loan]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanST_Loan] FOREIGN KEY([LoanID])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] CHECK CONSTRAINT [FK_LoanST_Loan]
GO
