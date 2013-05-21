IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_LoyaltyProgramActionType]') AND parent_object_id = OBJECT_ID(N'[dbo].[LoyaltyProgramActions]'))
ALTER TABLE [dbo].[LoyaltyProgramActions] DROP CONSTRAINT [FK_LoyaltyProgramActionType]
GO
ALTER TABLE [dbo].[LoyaltyProgramActions]  WITH CHECK ADD  CONSTRAINT [FK_LoyaltyProgramActionType] FOREIGN KEY([ActionTypeID])
REFERENCES [dbo].[LoyaltyProgramActionTypes] ([ActionTypeID])
GO
ALTER TABLE [dbo].[LoyaltyProgramActions] CHECK CONSTRAINT [FK_LoyaltyProgramActionType]
GO
