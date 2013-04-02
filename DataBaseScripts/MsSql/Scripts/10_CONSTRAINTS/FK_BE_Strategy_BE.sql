IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BE_Strategy_BE]') AND parent_object_id = OBJECT_ID(N'[dbo].[BusinessEntity_StrategyRel]'))
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] DROP CONSTRAINT [FK_BE_Strategy_BE]
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Strategy_BE] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] CHECK CONSTRAINT [FK_BE_Strategy_BE]
GO
