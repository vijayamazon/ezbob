IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailInsertStrategyVar]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailInsertStrategyVar]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_DetailInsertStrategyVar] 
	(@pApplicationId BIGINT,
	@pBodyId        int,
	@pDetailNameID  int,
	@pValueStr      NVARCHAR(MAX),
	@pDetailID  int output)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @id int;

	INSERT INTO [Application_Detail] WITH (ROWLOCK)
        ([ApplicationId] 
        ,[DetailNameId] 
        ,[ParentDetailId] 
        ,[ValueStr] 
        ,[ValueNum] 
        ,[ValueDateTime] 
        ,[IsBinary]) 
	VALUES ( @pApplicationId
            ,@pDetailNameID
            ,@pBodyId
            ,@pValueStr
            ,null
            ,null
            ,null);
            
  SET @id = @@IDENTITY;
  SET @pDetailID = @id;
  SELECT @id;
  RETURN @id;
END
GO
