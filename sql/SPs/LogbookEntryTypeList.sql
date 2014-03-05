IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LogbookEntryTypeList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[LogbookEntryTypeList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LogbookEntryTypeList]
AS
BEGIN
	SELECT
		lt.LogbookEntryTypeID,
		lt.LogbookEntryType,
		lt.LogbookEntryTypeDescription
	FROM
		LogbookEntryTypes lt
	ORDER BY
		CASE lt.LogbookEntryTypeID WHEN 0 THEN 1 ELSE 0 END,
		lt.LogbookEntryTypeDescription
END
GO
