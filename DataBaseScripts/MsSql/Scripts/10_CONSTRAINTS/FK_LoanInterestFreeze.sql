IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanInterestFreeze]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanInterestFreeze]'))
ALTER TABLE [dbo].[LoanInterestFreeze] DROP CONSTRAINT [FK_LoanInterestFreeze]
GO
ALTER TABLE [dbo].[LoanInterestFreeze]  WITH CHECK ADD  CONSTRAINT [FK_LoanInterestFreeze] FOREIGN KEY([LoanId])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanInterestFreeze] CHECK CONSTRAINT [FK_LoanInterestFreeze]
GO
