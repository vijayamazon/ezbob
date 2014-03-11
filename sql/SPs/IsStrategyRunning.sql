IF OBJECT_ID('IsStrategyRunning') IS NULL
	EXECUTE('CREATE PROCEDURE IsStrategyRunning AS SELECT 1')
GO

ALTER PROCEDURE IsStrategyRunning
@CustomerId INT,
@ActionName NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	DECLARE 
		@ActionNameId INT, 
		@ActionStatusId INT,
		@CurrentStatus BIT

	SELECT @ActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = @ActionName

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
		@CurrentStatus = IsInProgress 
	FROM 
		EzServiceActionStatus 
	WHERE 
		ActionStatusID = @ActionStatusId
	
	IF @CurrentStatus IS NULL
		SET @CurrentStatus = 0
		
	SELECT @CurrentStatus
END
GO