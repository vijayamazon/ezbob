DECLARE @UpdateMarketplacesActionId INT,
	@CustomerId INT,
	@EntryTime DATETIME,
	@MarketplaceId INT,
	@Comment VARCHAR(25),
	@UpdateMarketplaceActionId INT,
	@ServiceInstanceId INT,
	@Guid UNIQUEIDENTIFIER,
	@LastGuid UNIQUEIDENTIFIER,
	@ActionStatusId INT

CREATE TABLE #MarketplaceToGuid
(
	MarketplaceId INT,
	Guid UNIQUEIDENTIFIER
)

SELECT TOP 1 @ServiceInstanceId = InstanceID FROM EzServiceDefaultInstance ORDER BY EntryID ASC
SELECT @UpdateMarketplacesActionId = ActionNameID FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplaces'
SELECT @UpdateMarketplaceActionId = ActionNameID FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplace'

DECLARE customerCursor CURSOR FOR 
	SELECT DISTINCT
		CustomerId
	FROM 
		EzServiceActionHistory
	WHERE 
		ActionNameID = @UpdateMarketplacesActionId

OPEN customerCursor
FETCH NEXT FROM customerCursor INTO @CustomerId
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT TOP 1 @LastGuid = ActionID FROM EzServiceActionHistory WHERE ActionNameID = @UpdateMarketplacesActionId AND CustomerID = @CustomerId ORDER BY EntryTime DESC
	
	DECLARE entryCursor CURSOR FOR 
		SELECT
			EntryTime,
			ActionStatusID
		FROM 
			EzServiceActionHistory
		WHERE 
			ActionID = @LastGuid
		ORDER BY 
			EntryTime ASC
			
	OPEN entryCursor
	FETCH NEXT FROM entryCursor INTO @EntryTime, @ActionStatusId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE marketplaceCursor CURSOR FOR
			SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId AND Created < @EntryTime

		OPEN marketplaceCursor
		FETCH NEXT FROM marketplaceCursor INTO @MarketplaceId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @Guid = NULL
			SELECT @Guid = Guid FROM #MarketplaceToGuid WHERE MarketplaceId = @MarketplaceId
			IF @Guid IS NULL
			BEGIN
				SELECT @Guid = NEWID()
				INSERT INTO #MarketplaceToGuid VALUES (@MarketplaceId, @Guid)
			END
			
			SELECT @Comment = CONVERT(VARCHAR(10), @CustomerId) + '; ' + CONVERT(VARCHAR(10), @MarketplaceId)
			INSERT INTO EzServiceActionHistory
				(EntryTime, ActionID, IsSync, ActionStatusID, CurrentThreadID, UnderlyingThreadID, Comment, ActionNameID, ServiceInstanceID, UserID, CustomerID)
			VALUES
				(@EntryTime, @Guid, 0, @ActionStatusId, 1, 1, @Comment, @UpdateMarketplaceActionId, @ServiceInstanceId, NULL, @CustomerId)
		
			FETCH NEXT FROM marketplaceCursor INTO @MarketplaceId
		END
		CLOSE marketplaceCursor
		DEALLOCATE marketplaceCursor		
			
		FETCH NEXT FROM entryCursor INTO @EntryTime, @ActionStatusId
	END
	CLOSE entryCursor
	DEALLOCATE entryCursor
	
	
	FETCH NEXT FROM customerCursor INTO @CustomerId
END
CLOSE customerCursor
DEALLOCATE customerCursor

DROP TABLE #MarketplaceToGuid

DELETE FROM EzServiceActionHistory WHERE ActionNameID = @UpdateMarketplacesActionId
DELETE FROM EzServiceActionName WHERE ActionNameID = @UpdateMarketplacesActionId

GO







