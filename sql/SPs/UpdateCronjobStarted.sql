IF OBJECT_ID('UpdateCronjobStarted') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCronjobStarted AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateCronjobStarted
@JobID BiGINT,
@ActionStatusID INT,
@ActionID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @LogID BIGINT

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE EzServiceCrontab SET
		LastActionStatusID = @ActionStatusID,
		LastStartTime = @Now,
		LastEndTime = NULL
	WHERE
		JobID = @JobID

	------------------------------------------------------------------------------

	INSERT INTO EzServiceCronjobLog (JobID, ActionNameID, EntryTime, ActionStatusID, ActionID)
	SELECT
		JobID, ActionNameID, @Now, @ActionStatusID, @ActionID
	FROM
		EzServiceCrontab
	WHERE
		JobID = @JobID

	------------------------------------------------------------------------------

	SET @LogID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	INSERT INTO EzServiceCronjobLogArguments (EntryID, SerialNo, ArgumentTypeID, TypeHint, Value)
	SELECT
		@LogID, SerialNo, ArgumentTypeID, TypeHint, Value
	FROM
		EzServiceCronjobArguments
	WHERE
		JobID = @JobID

	------------------------------------------------------------------------------

	COMMIT TRANSACTION
END
GO
