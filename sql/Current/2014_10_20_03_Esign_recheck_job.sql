DECLARE @ActionNameID INT
DECLARE @JobID BIGINT

IF 1 < 0
BEGIN
	IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName LIKE 'EzBob.Backend.Strategies.Esign.EsignProcessPending')
		INSERT INTO EzServiceActionName (ActionName) VALUES ('EzBob.Backend.Strategies.Esign.EsignProcessPending')

	SELECT
		@ActionNameID = ActionNameID
	FROM
		EzServiceActionName
	WHERE
		ActionName LIKE 'EzBob.Backend.Strategies.Esign.EsignProcessPending'

	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
		VALUES(@ActionNameID, 0, 3, 'Oct 20 2014 0:10:0')

	SET @JobID = SCOPE_IDENTITY()

	INSERT INTO EzServiceCronjobArguments (JobID, SerialNo, ArgumentTypeID)
	SELECT
		@JobID,
		1,
		TypeID
	FROM
		EzServiceCronjobArgumentTypes
	WHERE
		TypeName = 'int'
		AND
		IsNullable = 1

	UPDATE EzServiceCrontab SET IsEnabled = 1 WHERE JobID = @JobID
END
GO
