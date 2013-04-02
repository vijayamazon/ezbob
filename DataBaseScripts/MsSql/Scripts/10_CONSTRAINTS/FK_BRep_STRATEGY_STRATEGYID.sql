IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BRep_STRATEGY_STRATEGYID]') AND parent_object_id = OBJECT_ID(N'[dbo].[BehavioralReports]'))
ALTER TABLE [dbo].[BehavioralReports] DROP CONSTRAINT [FK_BRep_STRATEGY_STRATEGYID]
GO
ALTER TABLE [dbo].[BehavioralReports]  WITH NOCHECK ADD  CONSTRAINT [FK_BRep_STRATEGY_STRATEGYID] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[BehavioralReports] CHECK CONSTRAINT [FK_BRep_STRATEGY_STRATEGYID]
GO
