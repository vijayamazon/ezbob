IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SV_ReportingInfo_Security_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[SV_ReportingInfo]'))
ALTER TABLE [dbo].[SV_ReportingInfo] DROP CONSTRAINT [FK_SV_ReportingInfo_Security_User]
GO
ALTER TABLE [dbo].[SV_ReportingInfo]  WITH CHECK ADD  CONSTRAINT [FK_SV_ReportingInfo_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[SV_ReportingInfo] CHECK CONSTRAINT [FK_SV_ReportingInfo_Security_User]
GO
