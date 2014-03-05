IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_NodeDataSign_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_NodeDataSign_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_NodeDataSign_Insert] 
	(@pApplicationId bigint,
	@pNodeId bigint,
	@pOutletName nvarchar(50),
	@pSignedData ntext,
	@pData ntext,
	@pNodeName nvarchar(250),
        @pUserName nvarchar(30))
AS
BEGIN
	INSERT INTO [Application_NodeDataSign]
           ([applicationId]
           ,[nodeId]
           ,[outletName]
           ,[signedData]
           ,[data]
           ,[dateAdded]
           ,[nodeName]
           ,[userName])
     VALUES
           (@pApplicationId
           ,@pNodeId
           ,@pOutletName
           ,@pSignedData
           ,@pData
           ,getDate()
           ,@pNodeName
           ,@pUserName)

SELECT SCOPE_IDENTITY();
END
GO
