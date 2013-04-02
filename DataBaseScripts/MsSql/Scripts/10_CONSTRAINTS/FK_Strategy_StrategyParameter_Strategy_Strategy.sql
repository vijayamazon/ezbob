IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_StrategyParameter_Strategy_Strategy]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyParameter]'))
ALTER TABLE [dbo].[Strategy_StrategyParameter] DROP CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_Strategy]
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_Strategy] FOREIGN KEY([OwnerId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter] CHECK CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_Strategy]
GO
