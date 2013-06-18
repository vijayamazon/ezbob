IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ExperianDefaultAccount_MP_ServiceLog]') AND parent_object_id = OBJECT_ID(N'[dbo].[ExperianDefaultAccount]'))
ALTER TABLE [dbo].[ExperianDefaultAccount] DROP CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog]
GO
ALTER TABLE [dbo].[ExperianDefaultAccount]  WITH CHECK ADD  CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog] FOREIGN KEY([ServiceLogId])
REFERENCES [dbo].[MP_ServiceLog] ([Id])
GO
ALTER TABLE [dbo].[ExperianDefaultAccount] CHECK CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog]
GO
