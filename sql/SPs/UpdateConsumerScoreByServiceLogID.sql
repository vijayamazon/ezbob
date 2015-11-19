SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UpdateConsumerScoreByServiceLogID') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateConsumerScoreByServiceLogID AS SELECT 1')
GO

ALTER PROCEDURE UpdateConsumerScoreByServiceLogID
@ServiceLogID BIGINT,
@BureauScore INT
AS
BEGIN
	DECLARE @CustomerID INT
	DECLARE @DirectorID INT

	SELECT
		@CustomerID = CustomerId,
		@DirectorID = DirectorId
	FROM
		MP_ServiceLog
	WHERE
		Id = @ServiceLogID

	IF @DirectorID IS NOT NULL
		UPDATE Director SET ExperianConsumerScore = @BureauScore WHERE Id = @DirectorID
	ELSE
		UPDATE Customer SET ExperianConsumerScore = @BureauScore WHERE Id = @CustomerID
END
GO
