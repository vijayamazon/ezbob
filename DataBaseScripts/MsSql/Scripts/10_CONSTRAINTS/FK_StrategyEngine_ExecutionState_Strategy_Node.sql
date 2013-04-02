IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_StrategyEngine_ExecutionState_Strategy_Node]') AND parent_object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecutionState]'))
ALTER TABLE [dbo].[StrategyEngine_ExecutionState] DROP CONSTRAINT [FK_StrategyEngine_ExecutionState_Strategy_Node]
GO
ALTER TABLE [dbo].[StrategyEngine_ExecutionState]  WITH NOCHECK ADD  CONSTRAINT [FK_StrategyEngine_ExecutionState_Strategy_Node] FOREIGN KEY([CurrentNodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[StrategyEngine_ExecutionState] CHECK CONSTRAINT [FK_StrategyEngine_ExecutionState_Strategy_Node]
GO
