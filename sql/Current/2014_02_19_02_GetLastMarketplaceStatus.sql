IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastMarketplaceStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastMarketplaceStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastMarketplaceStatus] 
	(@CustomerId INT,
	 @MarketplaceId INT)
AS
BEGIN
	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	DECLARE 
		@MpActionNameId INT,
		@MpsActionNameId INT, 
		@ActionStatusId INT,
		@CommentToSearch VARCHAR(25),
		@CurrentStatus VARCHAR(30)

	SELECT @MpActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplace'
	SELECT @MpsActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.UpdateMarketplaces'
	SELECT @CommentToSearch = CONVERT(VARCHAR(10), @CustomerId) + '; ' + CONVERT(VARCHAR(10), @MarketplaceId)
		
	SELECT 
		@ActionStatusId = ActionStatusId
	FROM
		(
			SELECT 
				row_number() over (partition by CustomerId order by EntryTime desc) rn, 
				ActionStatusId 
			FROM 
				EzServiceActionHistory 
			WHERE 
				(ActionNameID = @MpActionNameId AND CONVERT(VARCHAR(25), Comment) = @CommentToSearch) OR
				(ActionNameID = @MpsActionNameId AND CONVERT(VARCHAR(25), Comment) = CONVERT(VARCHAR(10), @CustomerId))
		) AS ActionsTable 
	WHERE 
		ActionsTable.rn = 1

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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplacesIsUpdated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_CustomerMarketplacesIsUpdated]
GO
