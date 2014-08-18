IF OBJECT_ID('SaveExperianHistory') IS NULL
	EXECUTE('CREATE PROCEDURE SaveExperianHistory AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveExperianHistory
 @ServiceLogId BIGINT
,@InsertDate DATETIME
,@Type NVARCHAR(50)
,@Score INT 
,@CustomerId INT = NULL
,@DirectorId INT = NULL
,@CompanyRefNum INT = NULL
,@Balance INT = NULL
,@CII INT = NULL
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM MP_ExperianHistory WHERE ServiceLogId=@ServiceLogId)
	BEGIN
		INSERT INTO MP_ExperianHistory (CustomerId, Type, [Date], ServiceLogId, Score, CII, CaisBalance, DirectorId, CompanyRefNum)
		VALUES (@CustomerId, @Type, @InsertDate, @ServiceLogId, @Score, @CII, @Balance, @DirectorId,@CompanyRefNum)
	END	
END
GO

