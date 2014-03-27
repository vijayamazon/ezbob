IF EXISTS (SELECT 1 FROM MP_MarketplaceType WHERE Name = 'CompanyFiles' AND GroupId IS NULL)
BEGIN
	DECLARE @GroupId INT
	SELECT @GroupId = Id FROM MP_MarketplaceGroup WHERE Name = 'Common'

	UPDATE MP_MarketplaceType SET PriorityOnline = -1, PriorityOffline = -1, GroupId = @GroupId WHERE Name = 'CompanyFiles'
	
	UPDATE MP_MarketplaceType SET PriorityOnline = PriorityOnline + 1 WHERE PriorityOnline != 100
	UPDATE MP_MarketplaceType SET PriorityOffline = PriorityOffline + 1 WHERE PriorityOffline != 100
END
GO
