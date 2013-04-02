IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanOptions_Loan]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanOptions]'))
ALTER TABLE [dbo].[LoanOptions] DROP CONSTRAINT [FK_LoanOptions_Loan]
GO
ALTER TABLE [dbo].[LoanOptions]  WITH CHECK ADD  CONSTRAINT [FK_LoanOptions_Loan] FOREIGN KEY([LoanId])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanOptions] CHECK CONSTRAINT [FK_LoanOptions_Loan]
GO
