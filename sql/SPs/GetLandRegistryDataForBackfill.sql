IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLandRegistryDataForBackfill]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLandRegistryDataForBackfill]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLandRegistryDataForBackfill]
AS
BEGIN
	SELECT 
		LandRegistry.CustomerId, 
		LandRegistry.Response,
		LandRegistry.TitleNumber,
		LandRegistry.Id
	FROM 
		LandRegistry,
		Customer
	WHERE 
		RequestType = 'Res' AND 
		ResponseType = 'Success' AND
		LandRegistry.CustomerId = Customer.Id
END
GO
