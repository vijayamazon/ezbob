IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DSSTR_STRATEGY_STRATEGYID]') AND parent_object_id = OBJECT_ID(N'[dbo].[DataSource_StrategyRel]'))
ALTER TABLE [dbo].[DataSource_StrategyRel] DROP CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID]
GO
ALTER TABLE [dbo].[DataSource_StrategyRel]  WITH NOCHECK ADD  CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[DataSource_StrategyRel] CHECK CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID]
GO
