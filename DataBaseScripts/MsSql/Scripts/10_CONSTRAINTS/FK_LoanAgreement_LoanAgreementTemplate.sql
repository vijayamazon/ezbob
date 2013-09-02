IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoanAgreement_LoanAgreementTemplate]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoanAgreement]'))
ALTER TABLE [dbo].[LoanAgreement] DROP CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate]
GO
ALTER TABLE [dbo].[LoanAgreement]  WITH CHECK ADD  CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[LoanAgreementTemplate] ([Id])
GO
ALTER TABLE [dbo].[LoanAgreement] CHECK CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate]
GO
