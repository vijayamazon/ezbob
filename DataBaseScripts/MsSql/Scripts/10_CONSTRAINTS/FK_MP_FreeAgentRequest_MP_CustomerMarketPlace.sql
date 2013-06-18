IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentRequest_MP_CustomerMarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentRequest]'))
ALTER TABLE [dbo].[MP_FreeAgentRequest] DROP CONSTRAINT [FK_MP_FreeAgentRequest_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_FreeAgentRequest]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentRequest_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentRequest] CHECK CONSTRAINT [FK_MP_FreeAgentRequest_MP_CustomerMarketPlace]
GO
