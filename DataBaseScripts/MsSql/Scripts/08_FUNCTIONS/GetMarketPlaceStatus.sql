IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketPlaceStatus]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetMarketPlaceStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[GetMarketPlaceStatus]
(	
  @marketplaceId INT, @customerid INT
)
RETURNS NVARCHAR(16)
AS BEGIN
	DECLARE @status NVARCHAR(64)
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
