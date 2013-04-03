IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyCheckIn]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyCheckIn]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyCheckIn] 
(
	@pStrategyName nvarchar(max),
	@pStrategyUserID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF NOT EXISTS( SELECT [displayname] FROM [Strategy_Strategy] WHERE [displayname] = @pStrategyName AND [IsDeleted] = 0)
	BEGIN
		RAISERROR( 'StrategyNoExist'
				 , 16
				 , 1
				 );
		RETURN;
	END;
	IF EXISTS( SELECT *
				   FROM [Strategy_Strategy]
				   WHERE [UserID] <> @pStrategyUserID
					 AND SubState = 0 /* Locked */
					 AND [displayname] = @pStrategyName
					 AND [IsDeleted] = 0 )
	BEGIN
		RAISERROR( 'StrategyIsLocked'
				 , 16
	  			 , 1
				 );
		RETURN;
	END;
	UPDATE [Strategy_Strategy]
       SET [UserID] = @pStrategyUserID,
           SubState = 1 --Unlocked
	 WHERE [displayname] = @pStrategyName
       AND State = 0 -- Challenge
  	   AND [IsDeleted] = 0;
END
GO
