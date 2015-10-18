SET QUOTED_IDENTIFIER ON
GO

DECLARE @Environment NVARCHAR(32) = 'Prod'
DECLARE @ActionName NVARCHAR(255) = 'Ezbob.Backend.Strategies.Tasks.UpdateDailyLoanStats'
DECLARE @IntTypeID INT = (SELECT TypeID FROM EzServiceCronjobArgumentTypes WHERE TypeName = 'int' AND IsNullable = 0)
DECLARE @RepetitionTypeID INT = (SELECT RepetitionTypeID FROM EzServiceCronjobRepetitionTypes WHERE RepetitionType = 'Daily')
DECLARE @JobID BIGINT
DECLARE @ActionNameID INT

IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Environment' AND Value = @Environment)
BEGIN
	IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName = @ActionName)
		INSERT INTO EzServiceActionName (ActionName) VALUES (@ActionName)

	SELECT @ActionNameID = ActionNameID FROM EzServiceActionName WHERE ActionName = @ActionName

	IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @ActionNameID)
	BEGIN
		INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
			VALUES (@ActionNameID, 1, @RepetitionTypeID, 'Oct 7 2015 1:30:00AM')

		SET @JobID = SCOPE_IDENTITY()

		INSERT INTO EzServiceCronjobArguments (JobID, SerialNo, ArgumentTypeID, Value)
			VALUES (@JobID, 0, @IntTypeID, '10')
	END
END
GO
