IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CompanyEmployeeCount_Customer]') AND parent_object_id = OBJECT_ID(N'[dbo].[CompanyEmployeeCount]'))
ALTER TABLE [dbo].[CompanyEmployeeCount] DROP CONSTRAINT [FK_CompanyEmployeeCount_Customer]
GO
ALTER TABLE [dbo].[CompanyEmployeeCount]  WITH CHECK ADD  CONSTRAINT [FK_CompanyEmployeeCount_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CompanyEmployeeCount] CHECK CONSTRAINT [FK_CompanyEmployeeCount_Customer]
GO
