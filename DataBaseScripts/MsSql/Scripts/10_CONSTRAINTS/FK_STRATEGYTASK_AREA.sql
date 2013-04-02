IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_STRATEGYTASK_AREA]') AND parent_object_id = OBJECT_ID(N'[dbo].[StrategyTasks]'))
ALTER TABLE [dbo].[StrategyTasks] DROP CONSTRAINT [FK_STRATEGYTASK_AREA]
GO
ALTER TABLE [dbo].[StrategyTasks]  WITH NOCHECK ADD  CONSTRAINT [FK_STRATEGYTASK_AREA] FOREIGN KEY([AreaId])
REFERENCES [dbo].[StrategyAreas] ([Id])
GO
ALTER TABLE [dbo].[StrategyTasks] CHECK CONSTRAINT [FK_STRATEGYTASK_AREA]
GO
