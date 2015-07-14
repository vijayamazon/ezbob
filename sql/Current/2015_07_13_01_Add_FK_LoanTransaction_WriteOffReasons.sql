IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanTransaction') AND name = 'WriteOffReasonID') 
	ALTER TABLE [dbo].[LoanTransaction] add [WriteOffReasonID] [INT] NULL DEFAULT NULL;		
GO
	
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanTransaction_WriteOffReasons') BEGIN
 ALTER TABLE [dbo].[LoanTransaction] ADD CONSTRAINT [FK_LoanTransaction_WriteOffReasons] FOREIGN KEY([WriteOffReasonID]) REFERENCES [dbo].[WriteOffReasons] ([WriteOffReasonID]);	
END
GO
