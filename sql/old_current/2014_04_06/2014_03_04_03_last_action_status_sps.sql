IF OBJECT_ID('GetLastMainStrategyStatus') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastMainStrategyStatus AS SELECT 1')
GO

ALTER PROCEDURE GetLastMainStrategyStatus
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	DECLARE 
		@ActionNameId INT, 
		@ActionStatusId INT,
		@CurrentStatus VARCHAR(30)

	SELECT @ActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.MainStrategy'

	SELECT TOP 1
		@ActionStatusId = ActionStatusId
	FROM
		EzServiceActionHistory
	WHERE
		ActionNameID = @ActionNameId
		AND
		CustomerID = @CustomerId
	ORDER BY
		EntryTime DESC

	SELECT 
		@CurrentStatus = ActionStatusName 
	FROM 
		EzServiceActionStatus 
	WHERE 
		ActionStatusID = @ActionStatusId
	
	IF @CurrentStatus IS NULL
		SET @CurrentStatus = 'Never Started'
		
	SELECT @CurrentStatus
END
GO

IF OBJECT_ID('GetLastMarketplaceStatus') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastMarketplaceStatus AS SELECT 1')
GO

ALTER PROCEDURE GetLastMarketplaceStatus
@CustomerId INT,
@MarketplaceId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	DECLARE 
		@MpActionNameId INT,
		@MpsActionNameId INT, 
		@ActionStatusId INT,
		@CommentToSearch VARCHAR(25),
		@CurrentStatus VARCHAR(30),
		@ActionID UNIQUEIDENTIFIER

	SELECT @MpActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplace'
	SELECT @MpsActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplaces'
	SELECT @CommentToSearch = CONVERT(VARCHAR(10), @CustomerId) + '; ' + CONVERT(VARCHAR(10), @MarketplaceId)
		
	SELECT TOP 1
		@ActionID = ActionID
	FROM
		EzServiceActionHistory
	WHERE 
		CustomerID = @CustomerId
		AND
		ActionStatusID IN (1, 7)
		AND
		(
			(ActionNameID = @MpActionNameId AND CONVERT(VARCHAR(25), Comment) = @CommentToSearch)
			OR
			(ActionNameID = @MpsActionNameId)
		)
	ORDER BY
		EntryTime DESC

	SELECT TOP 1
		@ActionStatusId = ActionStatusId
	FROM
		EzServiceActionHistory
	WHERE 
		ActionID = @ActionID
	ORDER BY
		EntryTime DESC

	SELECT 
		@CurrentStatus = ActionStatusName 
	FROM 
		EzServiceActionStatus 
	WHERE 
		ActionStatusID = @ActionStatusId
	
	IF @CurrentStatus IS NULL
		SET @CurrentStatus = 'Never Started'
		
	SELECT @CurrentStatus AS CurrentStatus
END
GO
