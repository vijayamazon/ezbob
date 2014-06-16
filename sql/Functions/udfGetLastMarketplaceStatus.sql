IF OBJECT_ID('dbo.udfGetLastMarketplaceStatus') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetLastMarketplaceStatus() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfGetLastMarketplaceStatus(@CustomerId INT, @MarketplaceId INT)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE 		
		@ActionStatusId INT,
		@CommentToSearch VARCHAR(25),
		@CurrentStatus VARCHAR(30),
		@ActionID UNIQUEIDENTIFIER

	SELECT @CommentToSearch = CONVERT(VARCHAR(10), @CustomerId) + '; ' + CONVERT(VARCHAR(10), @MarketplaceId) + '; %'

	SELECT TOP 1
		@ActionID = h.ActionID
	FROM
		EzServiceActionHistory h INNER JOIN EzServiceActionName a ON h.ActionNameID=a.ActionNameID
	WHERE 
		h.CustomerID = @CustomerId AND
		h.ActionStatusID IN (1, 7) AND
		a.ActionName IN ('EzBob.Backend.Strategies.UpdateMarketplace', 'EzBob.Backend.Strategies.Misc.UpdateMarketplace') AND
		CONVERT(VARCHAR(25), h.Comment) LIKE @CommentToSearch
	ORDER BY
		h.EntryTime DESC

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
