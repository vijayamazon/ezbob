
ALTER TABLE [dbo].[LoanAgreement]  WITH CHECK ADD CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[LoanAgreementTemplate] ([Id])
GO

