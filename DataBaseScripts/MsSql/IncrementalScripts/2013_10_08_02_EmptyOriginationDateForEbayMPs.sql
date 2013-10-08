UPDATE MP_CustomerMarketPlace
SET OriginationDate = NULL
WHERE MarketPlaceId=(SELECT Id FROM MP_MarketplaceType WHERE InternalId='A7120CB7-4C93-459B-9901-0E95E7281B59')