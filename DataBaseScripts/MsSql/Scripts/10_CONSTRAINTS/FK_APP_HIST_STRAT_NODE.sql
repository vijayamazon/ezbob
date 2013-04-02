IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APP_HIST_STRAT_NODE]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_History]'))
ALTER TABLE [dbo].[Application_History] DROP CONSTRAINT [FK_APP_HIST_STRAT_NODE]
GO
ALTER TABLE [dbo].[Application_History]  WITH NOCHECK ADD  CONSTRAINT [FK_APP_HIST_STRAT_NODE] FOREIGN KEY([CurrentNodeID])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[Application_History] CHECK CONSTRAINT [FK_APP_HIST_STRAT_NODE]
GO
