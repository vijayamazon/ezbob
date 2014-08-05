IF OBJECT_ID('GetFunctionAnalysisValuesByCustomerMarketPlace') IS NULL
	EXECUTE('CREATE PROCEDURE GetFunctionAnalysisValuesByCustomerMarketPlace AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetFunctionAnalysisValuesByCustomerMarketPlace
@MpID INT
AS
BEGIN
	SET NOCOUNT ON

	------------------------------------------------------------------------------

	SELECT
		Id,
		UpdatingStart
	INTO
		#h
	FROM
		MP_CustomerMarketplaceUpdatinghistory
	WHERE
		UpdatingStart IS NOT NULL
		AND
		UpdatingEnd IS NOT NULL
		AND
		CustomerMarketPlaceId = @MpID

	------------------------------------------------------------------------------

	SELECT
		f.InternalId AS fid, -- function id
		p.InternalId AS fpid, -- period id
		v.Value,
		h.UpdatingStart
	FROM
		MP_AnalyisisFunctionValues v
		INNER JOIN #h h ON v.CustomerMarketplaceUpdatingHistoryRecordId = h.id
		INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.id
		INNER JOIN MP_AnalysisFunctionTimeperiod p ON v.AnalysisFunctionTimeperiodId = p.id
	WHERE
		v.CustomerMarketPlaceId = @MpID

	------------------------------------------------------------------------------

	DROP TABLE #h
END
GO
