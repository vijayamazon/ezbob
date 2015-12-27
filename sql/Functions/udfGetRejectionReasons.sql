IF OBJECT_ID('dbo.udfGetRejectionReasons') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetRejectionReasons() RETURNS NVARCHAR(2000) AS BEGIN RETURN '''' END')
GO


SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION dbo.udfGetRejectionReasons(@DecisionHistoryID INT)
RETURNS NVARCHAR(2000)
AS
BEGIN
	DECLARE @RejectionReasons NVARCHAR(2000) = ''

	------------------------------------------------------------------------------

	SET @RejectionReasons = (
		SELECT rr.Reason + ',' AS 'data()'
		FROM DecisionHistoryRejectReason drr
		INNER JOIN RejectReason rr ON rr.Id = drr.RejectReasonId
		WHERE drr.DecisionHistoryId = @DecisionHistoryID
		FOR XML PATH('')
	)


	------------------------------------------------------------------------------
	RETURN @RejectionReasons
END

GO
