IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerLoyaltyProgramMP]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerLoyaltyProgram]'))
ALTER TABLE [dbo].[CustomerLoyaltyProgram] DROP CONSTRAINT [FK_CustomerLoyaltyProgramMP]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgramMP] FOREIGN KEY([CustomerMarketPlaceID])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgramMP]
GO
