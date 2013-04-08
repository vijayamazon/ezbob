IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EKMGetNewShops]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EKMGetNewShops]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE EKMGetNewShops	
AS
BEGIN
	DECLARE @LastHandledId INT
	SELECT @LastHandledId = CONVERT(INT, CfgValue) FROM EkmConnectorConfigs WHERE CfgKey='LastHandledId'

	SELECT 
		CustomerId,
		Id ShopId,
		DisplayName,
		cast(SecurityData as varchar(max)) as SecurityData  
	FROM MP_CustomerMarketPlace 
	WHERE 
		MarketPlaceId = 4 AND 
		Id > @LastHandledId

END
GO
