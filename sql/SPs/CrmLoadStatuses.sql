IF OBJECT_ID('CrmLoadStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE CrmLoadStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CrmLoadStatuses
AS
BEGIN
	SELECT
		s.Id AS StatusID,
		s.Name AS StatusName,
		g.Id AS GroupID,
		g.Name AS GroupName,
		g.Priority AS Priority,
		g.IsBroker AS IsBroker
	FROM
		CRMStatuses s
		INNER JOIN CRMStatusGroup g ON s.GroupId = g.Id
	ORDER BY
		g.Priority,
		s.Id
END
GO
