IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_AccountCheckedLoyalty]'))
DROP TRIGGER [dbo].[TR_AccountCheckedLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_AccountCheckedLoyalty
ON MP_AnalyisisFunctionValues
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	SELECT
		v.CustomerMarketPlaceId,
		MAX(
			((((CAST(YEAR(v.Updated) AS BIGINT)
			) * 100 + CAST(MONTH(v.Updated) AS BIGINT)
			) * 100 + CAST(DAY(v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(hour, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(minute, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(second, v.Updated) AS BIGINT)
		) AS MaxMeasureCode
	INTO
		#m
	FROM
		inserted v
		INNER JOIN MP_AnalyisisFunction f ON f.Id = v.AnalyisisFunctionId AND f.Name = 'TotalSumOfOrders'
		LEFT JOIN LoyaltyProgramCheckedAccounts lp
			ON v.CustomerMarketPlaceId = lp.CustomerMarketPlaceID
	WHERE
		v.ValueFloat > 0
		AND
		lp.CustomerMarketPlaceID IS NULL
	GROUP BY
		v.CustomerMarketPlaceId

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		mp.CustomerId,
		#m.CustomerMarketPlaceId,
		a.ActionID,
		a.Cost
	FROM
		#m
		INNER JOIN MP_CustomerMarketPlace mp ON #m.CustomerMarketPlaceId = mp.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'ACCOUNTCHECKED'
	
	DROP TABLE #m
	
	SET NOCOUNT OFF
END
GO
