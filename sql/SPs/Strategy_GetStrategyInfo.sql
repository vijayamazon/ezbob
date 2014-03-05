IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetStrategyInfo]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetStrategyInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetStrategyInfo] 
	(@pStrategyName nvarchar(max),
  @pIsEmbeddingAllowed int = NULL,
  @pIsPublished int = NULL,
  @pUserId int)
AS
BEGIN
	SELECT       s.StrategyId  AS StrategyId
             , s.DisplayName AS Name
             , s.Description AS Description
             , u.FullName AS Author
             , s.CreationDate AS PublishingDate
             , (select count(applicationid)  from application_application where
				strategyid = s.strategyid and state not in(2,3,0)) as ApplicationInProgress
             ,(SELECT count(applicationid) from application_application where
				strategyid = s.strategyid and state not in(2,3,0) and LockedByUserId <> @pUserId) as IsReadOnly
            , (select COUNT(publicid) from strategy_publicrel
				where strategyid = s.strategyid) AS IsPublished
FROM Strategy_Strategy s JOIN Security_User u ON s.AuthorID = u.UserID
WHERE  s.DisplayName = @pStrategyName and s.isdeleted = 0 AND s.TermDate IS NULL
END
GO
