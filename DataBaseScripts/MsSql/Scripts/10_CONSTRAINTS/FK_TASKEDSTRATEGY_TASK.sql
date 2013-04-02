IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TASKEDSTRATEGY_TASK]') AND parent_object_id = OBJECT_ID(N'[dbo].[TaskedStrategies]'))
ALTER TABLE [dbo].[TaskedStrategies] DROP CONSTRAINT [FK_TASKEDSTRATEGY_TASK]
GO
ALTER TABLE [dbo].[TaskedStrategies]  WITH NOCHECK ADD  CONSTRAINT [FK_TASKEDSTRATEGY_TASK] FOREIGN KEY([TaskId])
REFERENCES [dbo].[StrategyTasks] ([Id])
GO
ALTER TABLE [dbo].[TaskedStrategies] CHECK CONSTRAINT [FK_TASKEDSTRATEGY_TASK]
GO
