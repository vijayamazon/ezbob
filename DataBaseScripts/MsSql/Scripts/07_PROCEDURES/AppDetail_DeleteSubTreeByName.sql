IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_DeleteSubTreeByName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AppDetail_DeleteSubTreeByName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Evgen Nesterov
-- Create date: 23.06.2008
-- Description:	Delete all childs of given @RootDetailName
-- =============================================
CREATE PROCEDURE [dbo].[AppDetail_DeleteSubTreeByName]
	-- Add the parameters for the stored procedure here
	@pApplicationId  BIGINT,	
	@pRootDetailName   NVARCHAR( 512)
AS
BEGIN
    DECLARE @Depth INT
    SELECT @Depth = 0
    
    DECLARE @InsertedRowCount INT
    SELECT @InsertedRowCount = 0
    
	DECLARE @pRootDetailId INT
	SELECT @pRootDetailId =0  


    SELECT @pRootDetailId = d.detailid 
    FROM Application_Detail d INNER JOIN Application_Detail d1
      ON d.DetailId = d1.ParentDetailId
    WHERE d.applicationid = @pApplicationId 
    AND d.DetailNameID IN (SELECT detailnameID 
						 FROM Application_Detailname dn 
						 WHERE dn.name =  @pRootDetailName);

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
