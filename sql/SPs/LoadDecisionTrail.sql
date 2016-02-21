IF OBJECT_ID('LoadDecisionTrail') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDecisionTrail AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadDecisionTrail
@DiffID UNIQUEIDENTIFIER,
@TrailID BIGINT = NULL,
@ShowOldFlow BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SET @ShowOldFlow = ISNULL(@ShowOldFlow, 0)

	------------------------------------------------------------------------------

	IF @DiffID IS NULL
	BEGIN
		SELECT
			@DiffID = t.UniqueID
		FROM
			DecisionTrail t
		WHERE
			t.TrailID = @TrailID
	END

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Trail',
		trl.TrailID,
		trl.CustomerID,
		trl.CashRequestID,
		trl.NLCashRequestID,
		trl.DecisionID,
		trl.NLDecisionID,
		d.DecisionName,
		trl.DecisionTime,
		trl.UniqueID,
		trl.IsPrimary,
		trl.DecisionStatusID,
		s.DecisionStatus,
		trl.InputData
	FROM
		DecisionTrail trl
		INNER JOIN Decisions d ON trl.DecisionID = d.DecisionID
		INNER JOIN DecisionStatuses s ON trl.DecisionStatusID = s.DecisionStatusID
	WHERE
		trl.UniqueID = @DiffID
		AND (
			(@ShowOldFlow = 1 AND trl.IsPrimary IN (2, 3))
			OR
			(@ShowOldFlow = 0 AND trl.IsPrimary IN (0, 1))
		)
	ORDER BY
		trl.IsPrimary DESC

	------------------------------------------------------------------------------

	;WITH prim AS (
		SELECT
			trc.TraceID,
			trc.Position,
			n.TraceNameID,
			n.TraceName AS Name,
			trc.DecisionStatusID,
			s.DecisionStatus,
			trc.HasLockedDecision,
			trc.Comment
		FROM
			DecisionTrace trc
			INNER JOIN DecisionTrail trl
				ON trc.TrailID = trl.TrailID
				AND trl.IsPrimary = 1 + (@ShowOldFlow * 2)
				AND trl.UniqueID = @DiffID
			INNER JOIN DecisionStatuses s ON trc.DecisionStatusID = s.DecisionStatusID
			INNER JOIN DecisionTraceNames n ON trc.TraceNameID = n.TraceNameID
	), sec AS (
		SELECT
			trc.TraceID,
			trc.Position,
			n.TraceNameID,
			n.TraceName AS Name,
			trc.DecisionStatusID,
			s.DecisionStatus,
			trc.HasLockedDecision,
			trc.Comment
		FROM
			DecisionTrace trc
			INNER JOIN DecisionTrail trl
				ON trc.TrailID = trl.TrailID
				AND trl.IsPrimary = 0 + (@ShowOldFlow * 2)
				AND trl.UniqueID = @DiffID
			INNER JOIN DecisionStatuses s ON trc.DecisionStatusID = s.DecisionStatusID
			INNER JOIN DecisionTraceNames n ON trc.TraceNameID = n.TraceNameID
	)
	SELECT
		RowType = 'Trace',
		Position = ISNULL(prim.Position, sec.Position),
		--
		PrimID       = prim.TraceID,
		PrimNameID   = prim.TraceNameID,
		PrimName     = prim.Name,
		PrimStatusID = prim.DecisionStatusID,
		PrimStatus   = prim.DecisionStatus,
		PrimLocked   = prim.HasLockedDecision,
		PrimComment  = prim.Comment,
		--
		SameName     = CASE WHEN prim.TraceNameID = sec.TraceNameID THEN '' ELSE 'no' END,
		SameDecision = CASE WHEN prim.DecisionStatusID = sec.DecisionStatusID THEN '' ELSE 'no' END,
		--
		SecID        = sec.TraceID,
		SecNameID    = sec.TraceNameID,
		SecName      = sec.Name,
		SecStatusID  = sec.DecisionStatusID,
		SecStatus    = sec.DecisionStatus,
		SecLocked    = sec.HasLockedDecision,
		SecComment   = sec.Comment
	FROM
		prim
		FULL OUTER JOIN sec ON prim.Position = sec.Position
	ORDER BY
		ISNULL(prim.Position, sec.Position)

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Note',
		trl.IsPrimary,
		trl.TrailID,
		n.TrailNoteID,
		n.TrailNote
	FROM
		DecisionTrailNotes n
		INNER JOIN DecisionTrail trl
			ON n.TrailID = trl.TrailID
			AND trl.UniqueID = @DiffID
	ORDER BY
		trl.IsPrimary DESC,
		n.TrailNoteID
END
GO
