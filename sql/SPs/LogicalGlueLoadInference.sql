IF OBJECT_ID('LogicalGlueLoadInference') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInference AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LogicalGlueLoadInference
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SELECT
		rt.RequestTypeID,
		ResponseID = CONVERT(BIGINT, NULL)
	INTO
		#resp
	FROM
		LogicalGlueRequestTypes rt

	------------------------------------------------------------------------------

	UPDATE #resp SET ResponseID = ( 
		SELECT TOP 1
			r.ResponseID
		FROM
			LogicalGlueResponses r
			INNER JOIN MP_ServiceLog l
				ON r.ServiceLogID = l.Id
				AND l.CustomerId = @CustomerID
				AND l.InsertDate < @Now
				AND r.RequestTypeID = m.RequestTypeID
		ORDER BY
			l.InsertDate DESC,
			l.Id DESC
	)
	FROM #resp m

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Response',
		r.ResponseID,
		r.ServiceLogID,
		r.ReceivingTime,
		r.MonthlyRepayment,
		r.RequestTypeID,
		r.BucketID,
		r.InferenceResultEncoded,
		r.InferenceResultDecoded,
		r.Score,
		r.Status,
		r.Exception,
		r.ErrorCode,
		r.Uuid
	FROM
		LogicalGlueResponses r
		INNER JOIN #resp m ON r.ResponseID = m.ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'OutputRatio',
		r.OutputRatioID,
		r.ResponseID,
		r.OutputClass,
		r.Score
	FROM
		LogicalGlueResponseMapOutputRatios r
		INNER JOIN #resp m ON r.ResponseID = m.ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Warning',
		r.WarningID,
		r.ResponseID,
		r.Value,
		r.FeatureName,
		r.MinValue,
		r.MaxValue
	FROM
		LogicalGlueResponseWarnings r
		INNER JOIN #resp m ON r.ResponseID = m.ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'EncodingFailure',
		r.FailureID,
		r.ResponseID,
		r.RowIndex,
		r.ColumnName,
		r.UnencodedValue,
		r.Reason,
		r.Message
	FROM
		LogicalGlueResponseEncodingFailures r
		INNER JOIN #resp m ON r.ResponseID = m.ResponseID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'MissingColumn',
		r.MissingColumnID,
		r.ResponseID,
		r.ColumnName
	FROM
		LogicalGlueResponseMissingColumns r
		INNER JOIN #resp m ON r.ResponseID = m.ResponseID

	------------------------------------------------------------------------------

	DROP TABLE #resp
END
GO
