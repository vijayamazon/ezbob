IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerLoyaltyProgramAction]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerLoyaltyProgram]'))
ALTER TABLE [dbo].[CustomerLoyaltyProgram] DROP CONSTRAINT [FK_CustomerLoyaltyProgramAction]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgramAction] FOREIGN KEY([ActionID])
REFERENCES [dbo].[LoyaltyProgramActions] ([ActionID])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgramAction]
GO
