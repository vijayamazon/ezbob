IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanTransaction_MethodId]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanTransaction]'))
ALTER TABLE [dbo].[LoanTransaction] DROP CONSTRAINT [FK_LoanTransaction_MethodId]
GO
ALTER TABLE [dbo].[LoanTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanTransaction_MethodId] FOREIGN KEY([LoanTransactionMethodId])
REFERENCES [dbo].[LoanTransactionMethod] ([Id])
GO
ALTER TABLE [dbo].[LoanTransaction] CHECK CONSTRAINT [FK_LoanTransaction_MethodId]
GO
