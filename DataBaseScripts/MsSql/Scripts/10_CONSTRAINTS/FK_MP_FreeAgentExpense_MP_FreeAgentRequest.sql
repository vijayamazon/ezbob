IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentExpense_MP_FreeAgentRequest]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentExpense]'))
ALTER TABLE [dbo].[MP_FreeAgentExpense] DROP CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense] CHECK CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentRequest]
GO
