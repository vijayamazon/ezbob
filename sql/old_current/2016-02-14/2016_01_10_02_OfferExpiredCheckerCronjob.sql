SET QUOTED_IDENTIFIER ON
GO

DECLARE @Strategy NVARCHAR(500) = 'Ezbob.Backend.Strategies.Investor.OfferExpiredChecker'

IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName=@Strategy)
BEGIN
	INSERT INTO EzServiceActionName (ActionName) VALUES (@Strategy)
END 

DECLARE @ActionNameID INT = (SELECT ActionNameID FROM EzServiceActionName WHERE ActionName=@Strategy)
DECLARE @RepetitionTypeID INT = (SELECT RepetitionTypeID FROM EzServiceCronjobRepetitionTypes WHERE RepetitionType = 'Every X minutes')
DECLARE @JobID BIGINT
DECLARE @IsActive BIT = 0

IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @ActionNameID)
BEGIN
	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
	VALUES (@ActionNameID, @IsActive, @RepetitionTypeID, '2016-01-01 00:15:00')
END

GO