IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MenuItem_MenuItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[MenuItem]'))

ALTER TABLE dbo.MenuItem ADD CONSTRAINT
	FK_MenuItem_MenuItem FOREIGN KEY
	(
	ParentId
	) REFERENCES dbo.MenuItem
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO