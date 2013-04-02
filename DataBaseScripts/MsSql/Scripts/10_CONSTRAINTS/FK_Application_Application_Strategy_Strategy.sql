IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Application_Application_Strategy_Strategy]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_Application]'))
ALTER TABLE [dbo].[Application_Application] DROP CONSTRAINT [FK_Application_Application_Strategy_Strategy]
GO
ALTER TABLE [dbo].[Application_Application]  WITH NOCHECK ADD  CONSTRAINT [FK_Application_Application_Strategy_Strategy] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Application_Application] CHECK CONSTRAINT [FK_Application_Application_Strategy_Strategy]
GO
