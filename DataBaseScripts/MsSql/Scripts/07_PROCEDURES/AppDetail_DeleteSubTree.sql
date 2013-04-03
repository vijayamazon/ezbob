IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_DeleteSubTree]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AppDetail_DeleteSubTree]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Igor Borzenkov
-- Create date: 05.09.2007
-- Description:	Delete all childs of given @RootDetailId
-- =============================================
CREATE PROCEDURE [dbo].[AppDetail_DeleteSubTree]
	-- Add the parameters for the stored procedure here
	@pApplicationId  BIGINT,
	@pRootDetailId BIGINT
AS
BEGIN
    DECLARE @Depth INT
    SELECT @Depth = 0
    
    DECLARE @InsertedRowCount INT
    SELECT @InsertedRowCount = 0
    
    
    CREATE TABLE #Application_Detail
    ( [Depth]          INT           NOT NULL
    , [DetailId]       BIGINT        NOT NULL
    , [ParentDetailId] BIGINT        NULL
    )
    
    INSERT INTO #Application_Detail
    ( [Depth]
    , [DetailId]
    , [ParentDetailId]
    )
    SELECT @Depth
    	, [DetailId]      
    	, [ParentDetailId]
    FROM [Application_Detail]
    WHERE [ApplicationId] = @pApplicationId
    AND [DetailId] = @pRootDetailId
    
    SELECT @InsertedRowCount = @@RowCount
    
    WHILE @InsertedRowCount > 0
    BEGIN
    
    	INSERT INTO #Application_Detail
    	( [Depth]
    	, [DetailId]
    	, [ParentDetailId]
    	)
    	SELECT @Depth + 1
    		, [DetailId]      
    		, [ParentDetailId]
    	FROM [Application_Detail]
    	WHERE [ApplicationId] = @pApplicationId
    	AND [ParentDetailId] IN (SELECT [DetailId] FROM #Application_Detail WHERE [Depth] = @Depth )
    
    	SELECT @InsertedRowCount = @@RowCount
    	SELECT @Depth = @Depth + 1
    
    END
    
    DELETE FROM [Application_Detail]
     WHERE [DetailId] in (SELECT [DetailId] FROM #Application_Detail)
    
    DROP TABLE #Application_Detail
END
GO
