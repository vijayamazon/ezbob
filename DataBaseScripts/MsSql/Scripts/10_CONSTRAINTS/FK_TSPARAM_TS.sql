IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TSPARAM_TS]') AND parent_object_id = OBJECT_ID(N'[dbo].[TaskedStrategyParams]'))
ALTER TABLE [dbo].[TaskedStrategyParams] DROP CONSTRAINT [FK_TSPARAM_TS]
GO
ALTER TABLE [dbo].[TaskedStrategyParams]  WITH NOCHECK ADD  CONSTRAINT [FK_TSPARAM_TS] FOREIGN KEY([TSId])
REFERENCES [dbo].[TaskedStrategies] ([Id])
GO
ALTER TABLE [dbo].[TaskedStrategyParams] CHECK CONSTRAINT [FK_TSPARAM_TS]
GO
