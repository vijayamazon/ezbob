IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastMainStrategyStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastMainStrategyStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastMainStrategyStatus] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@ActionNameId INT, 
		@ActionStatusId INT,
		@CurrentStatus VARCHAR(30)

	SELECT @ActionNameId = ActionNameId FROM EzServiceActionName WHERE ActionName = 'EzBob.Backend.Strategies.MainStrategy'

	SELECT 
		@ActionStatusId = ActionStatusId
	FROM
		(SELECT row_number() over (partition by CustomerId order by EntryTime desc) rn, * FROM EzServiceActionHistory WHERE ActionNameID = @ActionNameId AND CustomerID = @CustomerId) AS ActionsTable 
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
		
	SELECT @CurrentStatus
END
GO
