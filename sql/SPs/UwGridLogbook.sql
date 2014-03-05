IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UwGridLogbook]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UwGridLogbook]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UwGridLogbook] 
	(@WithTest BIT)
AS
BEGIN
	SELECT
		l.EntryID,
		l.LogbookEntryTypeID,
		lt.LogbookEntryType,
		lt.LogbookEntryTypeDescription,
		l.EntryTime,
		l.UserID,
		u.UserName,
		u.FullName,
		l.EntryContent
	FROM
		Logbook l
		INNER JOIN LogbookEntryTypes lt ON l.LogbookEntryTypeID = lt.LogbookEntryTypeID
		INNER JOIN Security_User u ON l.UserID = u.UserId
	ORDER BY
		l.EntryTime DESC
END
GO
