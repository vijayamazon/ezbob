IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FKEA3BC91DC083FE87]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_CalendarRelation]'))
ALTER TABLE [dbo].[Strategy_CalendarRelation] DROP CONSTRAINT [FKEA3BC91DC083FE87]
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation]  WITH CHECK ADD  CONSTRAINT [FKEA3BC91DC083FE87] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation] CHECK CONSTRAINT [FKEA3BC91DC083FE87]
GO
