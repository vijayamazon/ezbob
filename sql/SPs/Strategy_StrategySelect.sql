IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategySelect]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategySelect]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategySelect] 
	(@pStrategyName nvarchar(400),
	@pStrategyUserId int,
	@pStrategyCheckOut bit,
	@pIsChampion bit)
AS
BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS( SELECT [Name] FROM [Strategy_Strategy] WHERE [Name] = @pStrategyName AND [IsDeleted] = 0)
	BEGIN
		RAISERROR( 'StrategyNoExist'
				 , 16
				 , 1
				 );
		RETURN;
	END
	
	IF EXISTS( SELECT *
		   FROM [Strategy_Strategy]
		   WHERE [UserID] <> @pStrategyUserID
			 AND [SubState] = 0 /* Locked */
			 AND [Name] = @pStrategyName
			 AND [IsDeleted] = 0
             AND 1 = @pStrategyCheckOut)
    BEGIN
		RAISERROR( 'StrategyIsLocked'
				 , 16
				 , 1
				 );
		RETURN
	END
    SELECT [Xml], SignedDocument
      FROM [Strategy_Strategy]
     WHERE [Name] = @pStrategyName
       AND [IsDeleted] = 0
END
GO
