IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyInsert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyInsert] 
	(@pStrategyName nvarchar(max)
    , @pStrategyDescription nvarchar(max)
    , @pStrategyIcon image
    , @pStrategyIsEmbeddingAllowed bit
    , @pStrategyXml ntext
    , @pStrategyUserID int
    , @pStrategyType nvarchar(255)
    , @pStrategyID int OUTPUT
    , @pSignedDocument ntext
    , @pDisplayName NVARCHAR(max)
    , @pRepublish BIGINT)
AS
BEGIN
	SET ANSI_WARNINGS OFF
	DECLARE @pOldState AS BIGINT;
	select @pOldState=[state] from strategy_strategy
     where displayname = @pDisplayName and termdate is null;
     
   update Strategy_Strategy set termdate = GETDATE(),
   substate = 1
   where displayname = @pDisplayName and termdate is null;
   
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    INSERT INTO [Strategy_Strategy]( [Name]
                 , [Description]
                 , [Icon]
                 , [IsEmbeddingAllowed] 
                 , [XML] 
                 , [UserID] 
                 , AuthorId
                 , DisplayName
                 , SignedDocument
                 , [StrategyType]
                 ) 
    VALUES( @pStrategyName 
          , @pStrategyDescription 
          , @pStrategyIcon 
          , @pStrategyIsEmbeddingAllowed 
          , @pStrategyXML 
          , @pStrategyUserID 
          , @pStrategyUserID
          , @pDisplayName
          , @pSignedDocument
          , @pStrategyType
          ) 
		
    SELECT @pStrategyID = SCOPE_IDENTITY();

    IF @pRepublish IS NOT NULL
    BEGIN
    	update strategy_publicrel set
		strategyid = @pStrategyID
		where strategyid in ( 
		  select strategyid from strategy_strategy where
		  displayname = @pDisplayName and termdate is not null);
		
		UPDATE Strategy_Strategy
		SET [State] = 0 
		WHERE displayname = @pDisplayName and termdate is not null;
			
		UPDATE Strategy_Strategy
		SET	[State] = @pOldState WHERE StrategyId=@pStrategyID;

    END
END
GO
