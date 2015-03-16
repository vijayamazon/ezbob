IF NOT EXISTS (SELECT * FROM syscolumns WHERE Id=object_id('EzbobMailNodeAttachRelation') AND name='UserID')
BEGIN
	ALTER TABLE EzbobMailNodeAttachRelation ADD UserID INT
END
GO
