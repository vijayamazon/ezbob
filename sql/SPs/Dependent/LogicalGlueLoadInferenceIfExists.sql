IF OBJECT_ID('LogicalGlueLoadInferenceIfExists') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInferenceIfExists AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LogicalGlueLoadInferenceIfExists
@CustomerID INT,
@Now DATETIME,
@IncludeTryOutData BIT,
@MonthlyPayment DECIMAL(18, 0)
AS
BEGIN
	SET @IncludeTryOutData = ISNULL(@IncludeTryOutData, 0)
	SET @MonthlyPayment = ISNULL(@MonthlyPayment, 0)

	------------------------------------------------------------------------------

	DECLARE @UniqueID UNIQUEIDENTIFIER = NULL
	DECLARE @MonthlyRepayment DECIMAL(18, 0) = NULL
	DECLARE @IsTryOut BIT = NULL
	DECLARE @ResponseID BIGINT = NULL

	------------------------------------------------------------------------------

	SELECT TOP 1
		@UniqueID = q.UniqueID,
		@MonthlyRepayment = q.MonthlyRepayment,
		@IsTryOut = q.IsTryOut,
		@ResponseID = r.ResponseID
	FROM
		LogicalGlueRequests q
		INNER JOIN MP_ServiceLog l
			ON q.ServiceLogID = l.Id
			AND l.CustomerId = @CustomerID
			AND l.InsertDate < @Now
		LEFT JOIN LogicalGlueResponses r ON l.Id = r.ServiceLogID
	WHERE (
			(@IncludeTryOutData = 0 AND q.IsTryOut = 0)
			OR
			(@IncludeTryOutData = 1)
		) AND (
			@MonthlyPayment < 0.01
			OR
			q.MonthlyRepayment = @MonthlyPayment
		)
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC

	------------------------------------------------------------------------------

	IF @ResponseID IS NULL
	BEGIN
		SELECT
			RowType = 'Request',
			UniqueID = @UniqueID,
			MonthlyRepayment = @MonthlyRepayment,
			IsTryOut = @IsTryOut
	END
	ELSE
		EXECUTE LogicalGlueLoadInference @ResponseID, @CustomerID, @Now, 1, @IncludeTryOutData, @MonthlyPayment
END
GO
