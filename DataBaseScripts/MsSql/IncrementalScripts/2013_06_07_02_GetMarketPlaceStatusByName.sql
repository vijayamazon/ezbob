IF object_id(N'GetMarketPlaceStatusByName', N'FN') IS NOT NULL
BEGIN
	DROP FUNCTION GetMarketPlaceStatusByName
END
GO

create FUNCTION [dbo].[GetMarketPlaceStatusByName]
(	
  @marketplaceName NVARCHAR(255), @customerid INT
)
RETURNS NVARCHAR(16)
AS BEGIN
	DECLARE @status NVARCHAR(64),
			@marketplaceId INT
	DECLARE @mt TABLE (
	MarketPlaceId INT,
	CustomerId INT,
	STATUS nvarchar (64),
	DisplayName NVARCHAR (64)
	)
	
	SELECT @marketplaceId = Id FROM MP_MarketplaceType WHERE Name = @marketplaceName

INSERT INTO @mt
SELECT MarketPlaceId, CustomerId, 
			   case when updateError is not null or updateError = ''  then 'Error'  
               when UpdatingStart is not null and UpdatingEnd is null then  'Updating' 
               when UpdatingStart is not null and UpdatingEnd is not null then  'Completed'  
			   END
			   'Status', DisplayName 
FROM [dbo].[MP_CustomerMarketPlace] where id IN 
(
	SELECT id  FROM MP_CustomerMarketPlace WHERE marketplaceId = @marketplaceId and customerid = @customerid

)

IF (select COUNT (*) FROM @mt WHERE STATUS='Error')>0 
set @status='Error'
ELSE
	IF (select COUNT (*) FROM @mt WHERE STATUS='Updating')>0
	SET @status= 'Updating'
	ELSE
		IF (select COUNT (*) FROM @mt WHERE STATUS='Completed')>0
		SET @status= 'Completed'
		ELSE 
			SET @status ='N/A'
RETURN @status
end

GO

