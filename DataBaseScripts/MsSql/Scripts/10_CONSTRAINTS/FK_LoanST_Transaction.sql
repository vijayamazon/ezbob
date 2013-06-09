IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanST_Transaction]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanScheduleTransaction]'))
ALTER TABLE [dbo].[LoanScheduleTransaction] DROP CONSTRAINT [FK_LoanST_Transaction]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanST_Transaction] FOREIGN KEY([TransactionID])
REFERENCES [dbo].[LoanTransaction] ([Id])
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] CHECK CONSTRAINT [FK_LoanST_Transaction]
GO
