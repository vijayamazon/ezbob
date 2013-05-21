IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerLoyaltyProgram]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerLoyaltyProgram]'))
ALTER TABLE [dbo].[CustomerLoyaltyProgram] DROP CONSTRAINT [FK_CustomerLoyaltyProgram]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgram] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgram]
GO
