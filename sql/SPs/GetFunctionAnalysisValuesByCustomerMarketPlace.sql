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

	SELECT
		f.InternalId AS fid, -- function id
		p.InternalId AS fpid, -- period id
		v.Value,
		v.Updated
	FROM
		MP_AnalyisisFunctionValues v
		INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.id
		INNER JOIN MP_AnalysisFunctionTimeperiod p ON v.AnalysisFunctionTimeperiodId = p.id
	WHERE
		v.CustomerMarketPlaceId = @MpID
END

GO

