IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_PublicSignInsert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_PublicSignInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_PublicSignInsert] 
	(@pStrategyPublicNameId bigint,
       @pAction nvarchar(7),
       @pUserId int,
       @pSignedDocument ntext,
       @pData ntext,
       @pStrategyId int,
	   @pAllData ntext)
AS
BEGIN
	if @psignedDocument is not null
      INSERT INTO Strategy_PublicSign
        (StrategyPublicId
        ,Action
        ,Data
        ,SignedDocument
        ,UserId
        ,StrategyId
		,AllData)
      VALUES
        (@pStrategyPublicNameId
        ,@pAction
        ,@pData
        ,@pSignedDocument
        ,@pUserId
        ,@pStrategyId
		,@pAllData)
END
GO
