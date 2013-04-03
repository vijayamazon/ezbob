IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailInsert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_DetailInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Igor Borzenkov
-- Create date: 05.09.2007
-- Description:	Insert's new record into Application_Detail
-- =============================================
CREATE PROCEDURE [dbo].[Application_DetailInsert]
	@pApplicationId  BIGINT,
	@pDetailNameID   INT,
	@pParentDetailId BIGINT,
	@pValueStr       NVARCHAR( MAX),
	@pIsBinary       BIT
AS
BEGIN
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

	 SELECT @@IDENTITY;
END
GO
