SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Misc.GetIncomeSms')
BEGIN
	INSERT INTO EzServiceActionName (ActionName) VALUES ('Ezbob.Backend.Strategies.Misc.GetIncomeSms')
END 


DECLARE @Environment NVARCHAR(32) = 'Prod'
DECLARE @ActionNameID INT = (SELECT ActionNameID FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Misc.GetIncomeSms')
DECLARE @BoolTypeID INT = (SELECT TypeID FROM EzServiceCronjobArgumentTypes WHERE TypeName = 'bool' AND IsNullable = 0)
DECLARE @DateTimeTypeID INT = (SELECT TypeID FROM EzServiceCronjobArgumentTypes WHERE TypeName = 'DateTime' AND IsNullable = 1)
DECLARE @RepetitionTypeID INT = (SELECT RepetitionTypeID FROM EzServiceCronjobRepetitionTypes WHERE RepetitionType = 'Daily')
DECLARE @JobID BIGINT
DECLARE @IsActive BIT = 0

IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Environment' AND Value = @Environment)
BEGIN
	SET @IsActive = 1
END

IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @ActionNameID)
BEGIN
	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
		VALUES (@ActionNameID, @IsActive, @RepetitionTypeID, '2014-01-01 06:00:00')

	SET @JobID = SCOPE_IDENTITY()

	INSERT INTO EzServiceCronjobArguments (JobID, SerialNo, ArgumentTypeID, Value, TypeHint)
		VALUES (@JobID, 0, @DateTimeTypeID, NULL, 'date is not passed')
	INSERT INTO EzServiceCronjobArguments (JobID, SerialNo, ArgumentTypeID, Value, TypeHint)
		VALUES (@JobID, 1, @BoolTypeID, 'true', 'use yesterday date')	
END

GO
