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