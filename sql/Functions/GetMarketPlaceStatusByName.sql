IF OBJECT_ID (N'dbo.GetMarketPlaceStatusByName') IS NOT NULL
	DROP FUNCTION dbo.GetMarketPlaceStatusByName
GO

CREATE FUNCTION [dbo].[GetMarketPlaceStatusByName]
(	@marketplaceName NVARCHAR(255), @customerid INT
)
RETURNS NVARCHAR(16) 
AS
BEGIN
	DECLARE @status NVARCHAR(64),
			@marketplaceId INT
	DECLARE @mt TABLE (
	MarketPlaceId INT,
	CustomerId INT,
	STATUS nvarchar (64),
	DisplayName NVARCHAR (64)
	)
	
	SELECT @marketplaceId = Id FROM MP_MarketplaceType WHERE Name = @marketplaceName

	SELECT @status =
(
	SELECT ISNULL(
		(select 
			CASE
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (mp.updateError is not null or mp.updateError = '') and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Error'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Updating'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is not null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Completed'
			END), 
		'N/A')
)
	RETURN @status
end

GO

