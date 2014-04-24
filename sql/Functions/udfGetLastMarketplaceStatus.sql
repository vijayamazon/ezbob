IF OBJECT_ID('dbo.udfGetLastMarketplaceStatus') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetLastMarketplaceStatus() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfGetLastMarketplaceStatus(@CustomerId INT, @MarketplaceId INT)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE 
		@MpActionNameId INT,
		@ActionStatusId INT,
		@CommentToSearch VARCHAR(25),
		@CurrentStatus VARCHAR(30),
		@ActionID UNIQUEIDENTIFIER

	SELECT @MpActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplace'
	SELECT @CommentToSearch = CONVERT(VARCHAR(10), @CustomerId) + '; ' + CONVERT(VARCHAR(10), @MarketplaceId) + '; %'

	SELECT TOP 1
		@ActionID = ActionID
	FROM
		EzServiceActionHistory
	WHERE 
		CustomerID = @CustomerId AND
		ActionStatusID IN (1, 7) AND
		ActionNameID = @MpActionNameId AND 
		CONVERT(VARCHAR(25), Comment) LIKE @CommentToSearch
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

	RETURN @CurrentStatus
END
GO
