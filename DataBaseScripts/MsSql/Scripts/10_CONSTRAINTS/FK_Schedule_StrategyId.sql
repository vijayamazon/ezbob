IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Schedule_StrategyId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_Schedule]'))
ALTER TABLE [dbo].[Strategy_Schedule] DROP CONSTRAINT [FK_Schedule_StrategyId]
GO
ALTER TABLE [dbo].[Strategy_Schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_Schedule_StrategyId] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_Schedule] CHECK CONSTRAINT [FK_Schedule_StrategyId]
GO
