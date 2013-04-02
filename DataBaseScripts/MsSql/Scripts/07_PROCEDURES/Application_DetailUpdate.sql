IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailUpdate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_DetailUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create date: 29.01.2008
-- Description:	Updates record if it exists in Application_Detail and Insertes if it doesn't exist 
-- Fixed when @pParentDetailId is NULL
-- 06.06.2008 by A.Grechko - fix of update with the same value 
-- =============================================
CREATE PROCEDURE [dbo].[Application_DetailUpdate]
	@pApplicationId  BIGINT,
	@pDetailNameID   BIGINT,
	@pParentDetailId BIGINT,
	@pValueStr       NVARCHAR( MAX),
	@pIsBinary       BIT
AS
BEGIN
	DECLARE @DetailId INT
	SELECT @DetailId = DetailId FROM Application_Detail WITH (NOLOCK) WHERE 
					ApplicationId = @pApplicationId AND
					DetailNameId  = @pDetailNameID AND
					(ParentDetailId = @pParentDetailId OR (ParentDetailId is NULL AND @pParentDetailId is NULL) );
	
-- Insert record
	IF (@DetailId IS NULL)
	begin	

		
	INSERT INTO [Application_Detail] WITH (ROWLOCK)
			   ([ApplicationId] 
			   ,[DetailNameId] 
			   ,[ParentDetailId] 
			   ,[ValueStr] 
			   ,[ValueNum] 
			   ,[ValueDateTime] 
			   ,[IsBinary]) 
		 VALUES 
			   ( @pApplicationId 
			   , @pDetailNameID 
			   , @pParentDetailId 
			   , @pValueStr 
			   , Null
			   , Null
			   , @pIsBinary); 

	select @@IDENTITY;
	return @@IDENTITY; -- FOR EXEC call. Needed refactoring, passing new Id should be performed via stored proc output param	
	end

	ELSE
	
-- Update record
	UPDATE Application_Detail WITH (ROWLOCK)
	SET ValueStr = @pValueStr
	WHERE DetailId = @DetailId;
	

 select @DetailId;
 return @DetailId; -- FOR EXEC call. Needed refactoring, passing new Id should be performed via stored proc output param	
	
END
GO
