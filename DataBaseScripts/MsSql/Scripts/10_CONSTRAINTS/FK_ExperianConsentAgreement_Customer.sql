IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ExperianConsentAgreement_Customer]') AND parent_object_id = OBJECT_ID(N'[dbo].[ExperianConsentAgreement]'))
ALTER TABLE [dbo].[ExperianConsentAgreement] DROP CONSTRAINT [FK_ExperianConsentAgreement_Customer]
GO
ALTER TABLE [dbo].[ExperianConsentAgreement]  WITH CHECK ADD  CONSTRAINT [FK_ExperianConsentAgreement_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[ExperianConsentAgreement] CHECK CONSTRAINT [FK_ExperianConsentAgreement_Customer]
GO
