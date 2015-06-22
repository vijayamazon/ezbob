-- add column to Customer and FK to ExternalCollectionStatuses
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'ExternalCollectionStatusID') 
	ALTER TABLE [dbo].[Customer] add [ExternalCollectionStatusID] [INT] NULL DEFAULT NULL;		
GO
	
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Customer_ExternalCollectionStatuses') BEGIN
 ALTER TABLE [dbo].[Customer] ADD CONSTRAINT [FK_Customer_ExternalCollectionStatuses] FOREIGN KEY([ExternalCollectionStatusID]) REFERENCES [dbo].[ExternalCollectionStatuses] ([ExternalCollectionStatusID]);	
END
GO
