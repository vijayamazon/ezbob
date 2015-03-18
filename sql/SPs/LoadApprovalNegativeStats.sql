SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadApprovalNegativeStats') IS NULL
	EXECUTE('CREATE PROCEDURE LoadApprovalNegativeStats AS SELECT 1')
GO

ALTER PROCEDURE LoadApprovalNegativeStats
@Tag NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @TagID BIGINT = (SELECT TrailTagID FROM DecisionTrailTags WHERE TrailTag = @Tag)

	SELECT
		t.TrailID,
		tc.TraceNameID
	FROM
		DecisionTrail t
		INNER JOIN DecisionTrace tc ON t.TrailID = tc.TrailID
		INNER JOIN DecisionTraceNames n
			ON tc.TraceNameID = n.TraceNameID
			AND ISNULL(n.IgnoreInGroupStats, 0) = 0
	WHERE
		t.DecisionID = 1 -- Approve
		AND
		t.DecisionStatusID != 1 -- Affirmative
		AND
		t.TrailTagID = @TagID
		AND
		tc.DecisionStatusID != 1 -- Affirmative
	ORDER BY
		t.TrailID,
		tc.TraceNameID
END
GO
