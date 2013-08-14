ALTER TABLE MP_CustomerMarketPlace
ADD AmazonMarketPlaceId INT

GO 

ALTER TABLE MP_CustomerMarketPlace
ADD CONSTRAINT FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType FOREIGN KEY (AmazonMarketPlaceId) REFERENCES dbo.MP_AmazonMarketplaceType (Id)

GO 

WITH new_id (amp_id, cmpid)
AS
(
 SELECT x.ampid, x.cmpid FROM 
(SELECT t.Id AS ampid,  c.Id AS cmpid
 FROM MP_AmazonMarketplaceType t, MP_AmazonOrder o, MP_AmazonOrderItem2Backup i, MP_CustomerMarketPlace c
 WHERE t.MarketplaceId = i.MarketplaceId
 AND o.Id = i.AmazonOrderId
 AND c.Id = o.CustomerMarketPlaceId
 GROUP BY c.Id, t.Id) x 
) 
UPDATE MP_CustomerMarketPlace SET AmazonMarketPlaceId = i.amp_id
FROM 
(
SELECT amp_id, cmpid 
FROM new_id
) i
WHERE Id=i.cmpid

GO
