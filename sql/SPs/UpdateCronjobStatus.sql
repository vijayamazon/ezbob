IF OBJECT_ID('UpdateCronjobStatus') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCronjobStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateCronjobStatus
@JobID BiGINT,
@ActionStatusID INT,
@ActionID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE EzServiceCrontab SET
		LastActionStatusID = @ActionStatusID
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

	COMMIT TRANSACTION
END
GO
