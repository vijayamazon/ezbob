ALTER TABLE ExperianDefaultAccount 
ADD ServiceLogId BIGINT
ALTER TABLE ExperianDefaultAccount 
ADD Balance INT
ALTER TABLE ExperianDefaultAccount 
ADD CurrentDefBalance INT

ALTER TABLE [dbo].[ExperianDefaultAccount]  WITH CHECK ADD CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog] FOREIGN KEY([ServiceLogId])
REFERENCES [dbo].[MP_ServiceLog] ([Id])
GO

ALTER TABLE [dbo].[ExperianDefaultAccount] CHECK CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog]
GO

CREATE NONCLUSTERED INDEX [IX_ServiceLogId] ON [dbo].[ExperianDefaultAccount]
(
	[ServiceLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO