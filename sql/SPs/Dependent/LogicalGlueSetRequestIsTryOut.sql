SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSetRequestIsTryOut') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSetRequestIsTryOut AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSetRequestIsTryOut
@RequestUniqueID UNIQUEIDENTIFIER,
@NewIsTryOutStatus BIT,
@Now DATETIME,
@RequestID BIGINT OUTPUT
AS
BEGIN
	SET @RequestID = NULL
	DECLARE @OldIsTryOutStatus BIT
	DECLARE @ServiceLogID BIGINT

	------------------------------------------------------------------------------

	SELECT
		@RequestID = RequestID,
		@OldIsTryOutStatus = IsTryOut,
		@ServiceLogID = ServiceLogID
	FROM
		LogicalGlueRequests
	WHERE
		UniqueID = @RequestUniqueID

	------------------------------------------------------------------------------

	IF @RequestID IS NULL
	BEGIN
		SET @RequestID = 0
		RETURN
	END

	------------------------------------------------------------------------------

	IF @OldIsTryOutStatus = @NewIsTryOutStatus
		RETURN

	------------------------------------------------------------------------------

	UPDATE LogicalGlueRequests SET
		IsTryOut = @NewIsTryOutStatus
	WHERE
		RequestID = @RequestID

	------------------------------------------------------------------------------

	DECLARE @ResponseID BIGINT = (
		SELECT
			rs.ResponseID
		FROM
			LogicalGlueResponses rs
		WHERE
			rs.ServiceLogID = @ServiceLogID
	)

	------------------------------------------------------------------------------

	IF @OldIsTryOutStatus = 0 AND @NewIsTryOutStatus = 1
	BEGIN
		EXECUTE LogicalGlueSaveCustomerHistory @ResponseID, @Now
		RETURN
	END

	------------------------------------------------------------------------------

	DECLARE @CustomerID INT
	DECLARE @CompanyID INT

	------------------------------------------------------------------------------

	SELECT
		@CustomerID = CustomerID,
		@CompanyID = CompanyID
	FROM
		MP_ServiceLog
	WHERE
		Id = @ServiceLogID

	------------------------------------------------------------------------------

	UPDATE CustomerLogicalGlueHistory SET
		IsActive = 0
	WHERE
		@CustomerID = CustomerID
		AND
		@CompanyID = CompanyID
		
	------------------------------------------------------------------------------

	DECLARE @EntryID BIGINT = (
		SELECT TOP 1
			EntryID
		FROM
			CustomerLogicalGlueHistory
		WHERE
			@CustomerID = CustomerID
			AND
			@CompanyID = CompanyID
			AND
			@ResponseID != ResponseID
		ORDER BY
			SetTime DESC,
			EntryID DESC
	)

	------------------------------------------------------------------------------

	IF @EntryID IS NOT NULL
	BEGIN
		INSERT INTO CustomerLogicalGlueHistory(
			CustomerID, CompanyID, ResponseID, IsActive, SetTime,
			GradeID, Score, IsHardReject, ScoreIsReliable, ErrorInResponse
		) SELECT
			CustomerID, CompanyID, ResponseID, 1, @Now,
			GradeID, Score, IsHardReject, ScoreIsReliable, ErrorInResponse
		FROM
			CustomerLogicalGlueHistory
		WHERE
			@EntryID = EntryID
	END
END
GO
