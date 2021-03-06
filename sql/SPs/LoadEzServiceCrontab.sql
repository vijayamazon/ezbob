IF OBJECT_ID('LoadEzServiceCrontab') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEzServiceCrontab AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEzServiceCrontab
@IncludeRunning BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		n.ActionName,
		t.JobID,
		t.ActionNameID,
		t.RepetitionTypeID,
		t.RepetitionTime,
		t.LastActionStatusID,
		t.LastStartTime,
		t.LastEndTime,
		ISNULL(s.IsInProgress, 0) AS IsInProgress,
		a.ArgumentID,
		a.SerialNo,
		a.Value,
		a.TypeHint,
		at.TypeName,
		at.IsNullable,
		at.TypeID
	FROM
		EzServiceCrontab t
		INNER JOIN EzServiceActionName n ON t.ActionNameID = n.ActionNameID
		LEFT JOIN EzServiceActionStatus s ON t.LastActionStatusID = s.ActionStatusID
		LEFT JOIN EzServiceCronjobArguments a ON t.JobID = a.JobID
		LEFT JOIN EzServiceCronjobArgumentTypes at ON a.ArgumentTypeID = at.TypeID
	WHERE
		t.IsEnabled = 1
		AND (
			@IncludeRunning = 1
			OR
			t.LastActionStatusID IS NULL
			OR
			s.IsInProgress != 1
		)
	ORDER BY
		t.JobID,
		a.SerialNo
END
GO
