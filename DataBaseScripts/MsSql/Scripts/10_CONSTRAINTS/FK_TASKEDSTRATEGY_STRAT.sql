IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TASKEDSTRATEGY_STRAT]') AND parent_object_id = OBJECT_ID(N'[dbo].[TaskedStrategies]'))
ALTER TABLE [dbo].[TaskedStrategies] DROP CONSTRAINT [FK_TASKEDSTRATEGY_STRAT]
GO
ALTER TABLE [dbo].[TaskedStrategies]  WITH NOCHECK ADD  CONSTRAINT [FK_TASKEDSTRATEGY_STRAT] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[TaskedStrategies] CHECK CONSTRAINT [FK_TASKEDSTRATEGY_STRAT]
GO
