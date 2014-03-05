IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LogbookAdd]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[LogbookAdd]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LogbookAdd] 
	(@LogbookEntryTypeID INT,
@UserID INT,
@EntryContent NTEXT)
AS
BEGIN
	INSERT INTO Logbook (LogbookEntryTypeID, UserID, EntryContent)
		VALUES (@LogbookEntryTypeID, @UserID, @EntryContent)

	RETURN SCOPE_IDENTITY()
END
GO
