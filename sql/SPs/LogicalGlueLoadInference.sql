IF OBJECT_ID('LogicalGlueLoadInference') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInference AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LogicalGlueLoadInference
@ResponseID BIGINT,
@CustomerID INT,
@Now DATETIME,
@IncludeTryOutData BIT,
@MonthlyPayment DECIMAL(18, 0)
AS
BEGIN
	IF @ResponseID <= 0
	BEGIN
		SET @ResponseID = (
			SELECT TOP 1
				r.ResponseID
			FROM
				LogicalGlueResponses r
				INNER JOIN MP_ServiceLog l
					ON r.ServiceLogID = l.Id
					AND l.CustomerId = @CustomerID
					AND l.InsertDate < @Now
				INNER JOIN LogicalGlueRequests rr ON l.Id = rr.ServiceLogID
			WHERE
				(@IncludeTryOutData = 0 AND rr.IsTryOut = 0)
				OR
				(@IncludeTryOutData = 1 AND (@MonthlyPayment < 0.01 OR rr.MonthlyRepayment = @MonthlyPayment))
			ORDER BY
				l.InsertDate DESC,
				l.Id DESC
		)
	END

	------------------------------------------------------------------------------

	SELECT
		m.ModelOutputID
	INTO
		#models
	FROM
		LogicalGlueModelOutputs m
	WHERE
		m.ResponseID = @ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Response',
		r.ResponseID,
		r.ServiceLogID,
		r.ReceivedTime,
		r.HttpStatus,
		r.ResponseStatus,
		r.TimeoutSourceID,
		r.ErrorMessage,
		r.GradeID,
		r.HasEquifaxData,
		r.ParsingExceptionType,
		r.ParsingExceptionMessage,
		q.UniqueID,
		q.MonthlyRepayment
	FROM
		LogicalGlueResponses r
		INNER JOIN LogicalGlueRequests q ON r.ServiceLogID = q.ServiceLogID
	WHERE
		r.ResponseID = @ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'ModelOutput',
		r.ModelOutputID,
		r.ResponseID,
		r.ModelID,
		r.InferenceResultEncoded,
		r.InferenceResultDecoded,
		r.Score,
		r.Status,
		r.Exception,
		r.ErrorCode,
		r.Uuid
	FROM
		LogicalGlueModelOutputs r
		INNER JOIN #models m ON r.ModelOutputID = m.ModelOutputID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'OutputRatio',
		r.OutputRatioID,
		r.ModelOutputID,
		r.OutputClass,
		r.Score
	FROM
		LogicalGlueModelOutputRatios r
		INNER JOIN #models m ON r.ModelOutputID = m.ModelOutputID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Warning',
		r.WarningID,
		r.ModelOutputID,
		r.Value,
		r.FeatureName,
		r.MinValue,
		r.MaxValue
	FROM
		LogicalGlueModelWarnings r
		INNER JOIN #models m ON r.ModelOutputID = m.ModelOutputID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'EncodingFailure',
		r.FailureID,
		r.ModelOutputID,
		r.RowIndex,
		r.ColumnName,
		r.UnencodedValue,
		r.Reason,
		r.Message
	FROM
		LogicalGlueModelEncodingFailures r
		INNER JOIN #models m ON r.ModelOutputID = m.ModelOutputID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'MissingColumn',
		r.MissingColumnID,
		r.ModelOutputID,
		r.ColumnName
	FROM
		LogicalGlueModelMissingColumns r
		INNER JOIN #models m ON r.ModelOutputID = m.ModelOutputID

	------------------------------------------------------------------------------

	DROP TABLE #models
END
GO
