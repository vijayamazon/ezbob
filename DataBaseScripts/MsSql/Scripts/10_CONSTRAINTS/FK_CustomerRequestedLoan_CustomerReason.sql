IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerRequestedLoan_CustomerReason]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerRequestedLoan]'))
ALTER TABLE [dbo].[CustomerRequestedLoan] DROP CONSTRAINT [FK_CustomerRequestedLoan_CustomerReason]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRequestedLoan_CustomerReason] FOREIGN KEY([ReasonId])
REFERENCES [dbo].[CustomerReason] ([Id])
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] CHECK CONSTRAINT [FK_CustomerRequestedLoan_CustomerReason]
GO
