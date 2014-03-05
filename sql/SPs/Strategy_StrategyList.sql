IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyList] 
	(@pUserId int,
    @pIsPublished bit = NULL,
    @pIsEmbeddingAllowed bit = NULL)
AS
BEGIN
	SET NOCOUNT ON;
  
       SELECT s.[StrategyId]                      AS [StrategyId]
            , s.[Name]                            AS [Name]
            , s.[Description]                     AS [Description]
            , u.[FullName]                        AS [Author]
            , s.[CreationDate]                    AS [PublishingDate],
            s.TermDate AS [TermDate],
            s.DisplayName AS [DisplayName]
            , case s.[SubState]
                 when 0 /* Locked */ then case s.[UserId]
                                             when @pUserId then 0
                                             else 1
                                           end
                 when 1 then 0
                 else 1
              end                                 AS [IsReadOnly]
            , s.[IsEmbeddingAllowed]              AS [IsEmbeddingAllowed]
            , case State
                 when 1 then 1
                 else 0
              end                                 AS [IsPublished]
            , s.[Icon]				  AS [Icon]
            , s.[UserId]			  AS [UserId]
            , s.[IsMigrationSupported]		  AS [IsMigrationSupported]
         FROM [Strategy_Strategy] AS s JOIN [Security_User] AS u ON s.[AuthorID] = u.[UserID]
        WHERE ( s.[IsEmbeddingAllowed] = @pIsEmbeddingAllowed OR @pIsEmbeddingAllowed IS NULL )
          AND ( s.[IsDeleted] = 0)
       ORDER BY s.displayname, s.termdate desc
END
GO
