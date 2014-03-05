IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_DeleteSubTree]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AppDetail_DeleteSubTree]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppDetail_DeleteSubTree] 
	(@pApplicationId  BIGINT,
	@pRootDetailId BIGINT)
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
