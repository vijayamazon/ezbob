IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CashRequests_DiscountPlan]') AND parent_object_id = OBJECT_ID(N'[dbo].[CashRequests]'))
ALTER TABLE [dbo].[CashRequests] DROP CONSTRAINT [FK_CashRequests_DiscountPlan]
GO
ALTER TABLE [dbo].[CashRequests]  WITH CHECK ADD  CONSTRAINT [FK_CashRequests_DiscountPlan] FOREIGN KEY([DiscountPlanId])
REFERENCES [dbo].[DiscountPlan] ([Id])
GO
ALTER TABLE [dbo].[CashRequests] CHECK CONSTRAINT [FK_CashRequests_DiscountPlan]
GO
