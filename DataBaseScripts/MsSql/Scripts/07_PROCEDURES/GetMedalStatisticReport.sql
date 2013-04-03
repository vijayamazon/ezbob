IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMedalStatisticReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMedalStatisticReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMedalStatisticReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	select 
			Medals.Medal,
			ISNULL( EbayStoresCount, 0) EbayStoresCount, 
			ISNULL( EbayStoresAverage, 0) EbayStoresAverage, 
			ISNULL( PayPalStoresCount, 0) PayPalStoresCount, 
			ISNULL( PayPalStoresAverage, 0) PayPalStoresAverage, 
			ISNULL( AmazonStoresCount, 0) AmazonStoresCount, 
			ISNULL( AmazonStoresAverage, 0) AmazonStoresAverage, 
			ISNULL( ScorePointsMin, 0) ScorePointsMin,
			ISNULL( ScorePointsMax, 0) ScorePointsMax,
			ISNULL( ScorePointsAverage, 0) ScorePointsAverage,
			ISNULL( ExperianRatingMin, 0) ExperianRatingMin,
			ISNULL( ExperianRatingMax, 0) ExperianRatingMax,
			ISNULL( ExperianRatingAverage, 0) ExperianRatingAverage,
			ISNULL( AnualTurnoverMin, 0) AnualTurnoverMin,
			ISNULL( AnualTurnoverMax, 0) AnualTurnoverMax,
			ISNULL( AnualTurnoverAverage, 0) AnualTurnoverAverage,
			ISNULL( CustomersCount, 0) CustomersCount,
			ISNULL( AmazonReviews, 0) AmazonReviews,
			ISNULL( AmazonRating, 0) AmazonRating,
			ISNULL( EbayReviews, 0) EbayReviews,
			ISNULL( EbayRating, 0) EbayRating
	 from
	 Medals LEFT OUTER JOIN 
	(select Medal, 
			SUM(ebayStores) EbayStoresCount, 
			AVG(Cast(ebayStores as Float)) as EbayStoresAverage, 
			SUM(payPalStores) PayPalStoresCount, 
			AVG(Cast(payPalStores as float)) as PayPalStoresAverage, 
			SUM(amazonStores) AmazonStoresCount, 
			AVG(CAST(amazonStores as float))as AmazonStoresAverage, 
			MIN(ScorePoints) ScorePointsMin,
			MAX(ScorePoints) ScorePointsMax,
			AVG(ScorePoints) ScorePointsAverage,
			MIN(ExperianRating) ExperianRatingMin,
			MAX(ExperianRating) ExperianRatingMax,
			AVG(ExperianRating) ExperianRatingAverage,
			MIN(AnualTurnover) AnualTurnoverMin,
			MAX(AnualTurnover) AnualTurnoverMax,
			AVG(AnualTurnover) AnualTurnoverAverage,
			Count(IdCustomer) CustomersCount,
			AVG(AmazonReviews) AmazonReviews,
			AVG(AmazonRating) AmazonRating,
			AVG(EbayReviews) EbayReviews,
			AVG(EbayRating) EbayRating
	FROM
	(select UPPER(MedalType) Medal, c.IdCustomer, 
		ISNULL(es.StoresCount, 0) as ebayStores, 
		ISNULL(ps.StoresCount, 0) as payPalStores, 
		ISNULL(ams.StoresCount, 0) as amazonStores ,
		ISNULL(c.ScorePoints, 0) as ScorePoints, 
		ISNULL(c.ExperianRating, 0) as ExperianRating, 
		ISNULL(c.AnualTurnover, 0) as AnualTurnover,
		ISNULL(ar.Reviews, 0) as AmazonReviews,
		ISNULL(ar.Rating, 0) as AmazonRating,
		ISNULL(ar.Reviews, 0) as EbayReviews,
		ISNULL(er.Rating, 0) as EbayRating
		from 
	(SELECT DISTINCT CashRequests.MedalType, CashRequests.IdCustomer, CashRequests.ScorePoints, CashRequests.ExpirianRating as ExperianRating, CashRequests.AnualTurnover
		FROM CashRequests
		INNER JOIN 
		(SELECT MAX(CreationDate) CreationDate, IdCustomer FROM CashRequests
			WHERE CreationDate >= @dateStart
			AND CreationDate <= @dateEnd
			GROUP By (IdCustomer)) as cr ON cr.CreationDate = CashRequests.CreationDate AND cr.IdCustomer = CashRequests.IdCustomer
		WHERE MedalType IS NOT NULL
	) as c 
	LEFT OUTER JOIN (SELECT * FROM GETSTORESCOUNT(1, @dateStart, @dateEnd)) as es 
	ON c.IdCustomer = es.CustomerId	
	LEFT OUTER JOIN	(SELECT * FROM GETSTORESCOUNT(2, @dateStart, @dateEnd))as ams 
	ON c.IdCustomer = ams.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GETSTORESCOUNT(3, @dateStart, @dateEnd))as ps 
	ON c.IdCustomer = ps.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GetAmazonReviews() )as ar 
	ON c.IdCustomer = ar.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GetEbayReviews() )as er 
	ON c.IdCustomer = er.CustomerId) as m
	GROUP BY Medal) as r
	ON UPPER(Medals.Medal) = UPPER(r.Medal)
END
GO
