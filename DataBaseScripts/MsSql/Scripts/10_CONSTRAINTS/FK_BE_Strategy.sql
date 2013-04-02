IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BE_Strategy]') AND parent_object_id = OBJECT_ID(N'[dbo].[BusinessEntity_StrategyRel]'))
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] DROP CONSTRAINT [FK_BE_Strategy]
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Strategy] FOREIGN KEY([BusinessEntityId])
REFERENCES [dbo].[BusinessEntity] ([Id])
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] CHECK CONSTRAINT [FK_BE_Strategy]
GO
