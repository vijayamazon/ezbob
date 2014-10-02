IF OBJECT_ID('RptStrategyRunningTime') IS NULL
	EXECUTE ('CREATE PROCEDURE RptStrategyRunningTime AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptStrategyRunningTime
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		h.ActionID,
		h.ActionNameID,
		n.ActionName,
		h.EntryTime,
		h.ActionStatusID,
		h.IsSync
	FROM
		EzServiceActionHistory h
		INNER JOIN EzServiceActionName n ON h.ActionNameID = n.ActionNameID
	WHERE
		n.ActionName NOT LIKE 'Admin%'
		AND
		h.ActionStatusID != 7
		AND
		@DateStart <= h.EntryTime AND h.EntryTime < @DateEnd
END
GO
