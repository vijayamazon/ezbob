IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentExpense]'))
ALTER TABLE [dbo].[MP_FreeAgentExpense] DROP CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory]
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[MP_FreeAgentExpenseCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense] CHECK CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory]
GO
