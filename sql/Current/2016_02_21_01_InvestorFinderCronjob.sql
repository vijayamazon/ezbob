SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Tasks.InvestorFinder')
BEGIN
	INSERT INTO EzServiceActionName (ActionName) VALUES ('Ezbob.Backend.Strategies.Tasks.InvestorFinder')
END 


DECLARE @Environment NVARCHAR(32) = 'Prod'
DECLARE @ActionNameID INT = (SELECT ActionNameID FROM EzServiceActionName WHERE ActionName='Ezbob.Backend.Strategies.Tasks.InvestorFinder')
DECLARE @RepetitionTypeID INT = (SELECT RepetitionTypeID  FROM EzServiceCronjobRepetitionTypes WHERE RepetitionType = 'Every X minutes')
DECLARE @JobID BIGINT
DECLARE @IsActive BIT = 0

IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @ActionNameID)
BEGIN
	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
		VALUES (@ActionNameID, @IsActive, @RepetitionTypeID, '2016-01-01 01:30:00')
END

GO
