IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentCompany_MP_FreeAgentRequest]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentCompany]'))
ALTER TABLE [dbo].[MP_FreeAgentCompany] DROP CONSTRAINT [FK_MP_FreeAgentCompany_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_FreeAgentCompany]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentCompany_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentCompany] CHECK CONSTRAINT [FK_MP_FreeAgentCompany_MP_FreeAgentRequest]
GO
