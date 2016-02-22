SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Tasks.Annual77ANotifier')
BEGIN
	INSERT INTO EzServiceActionName (ActionName) VALUES ('Ezbob.Backend.Strategies.Tasks.Annual77ANotifier')
END 


DECLARE @Environment NVARCHAR(32) = 'Prod'
DECLARE @ActionNameID INT = (SELECT ActionNameID FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Tasks.Annual77ANotifier')
DECLARE @RepetitionTypeID INT = (SELECT RepetitionTypeID FROM EzServiceCronjobRepetitionTypes WHERE RepetitionType = 'Daily')
DECLARE @JobID BIGINT
DECLARE @IsActive BIT = 0

IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @ActionNameID)
BEGIN
	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
		VALUES (@ActionNameID, @IsActive, @RepetitionTypeID, '2015-01-01 11:00:00')
END

GO
