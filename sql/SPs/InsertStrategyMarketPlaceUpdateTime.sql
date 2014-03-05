IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyMarketPlaceUpdateTime]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertStrategyMarketPlaceUpdateTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertStrategyMarketPlaceUpdateTime] 
	(@MarketPlaceId int,
 @StartDate datetime,
 @EndDate datetime)
AS
BEGIN
	insert INTO  [dbo].[Strategy_MarketPlaceUpdateHistory] (MarketPlaceId, StartDate, EndDate)
     VALUES (@MarketPlaceId, @StartDate, @EndDate);

SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
