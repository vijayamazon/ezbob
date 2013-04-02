IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_NodeStrategyRel_Strategy_Strategy]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRel]'))
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] DROP CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Strategy]
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Strategy] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] CHECK CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Strategy]
GO
