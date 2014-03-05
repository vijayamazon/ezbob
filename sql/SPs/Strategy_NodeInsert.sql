IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeInsert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_NodeInsert] 
	(@pName nvarchar(max),
       @pDisplayName nvarchar(max),
       @pDescription nvarchar(max),
       @pExecutionDuration bigint,
       @pIcon image,
       @pGroupId tinyint,
       @pContainsPrint bigint,
       @pNodeId int OUTPUT,
       @pCustomUrl NVARCHAR(MAX),
       @pGUID NVARCHAR(MAX),
       @pComment NVARCHAR(MAX),
       @pNDX image,
       @pSignedDocument ntext,
	   @pUserId int)
AS
BEGIN
	-- Terminate current nodes
     update Strategy_Node set TerminationDate  = GETDATE()
     where upper([Name]) = upper(@pName) and TerminationDate is null;

     insert into Strategy_Node
     (
       Name,
       DisplayName,
       Description,
       ExecutionDuration,
       Icon,
       GroupId,
       ContainsPrint,
       CustomUrl,
       [guid],
       nodecomment,
       NDX,
       SignedDocument,
       CreatorUserId
     )
     values
     (
       @pName,
       @pDisplayName,
       @pDescription,
       @pExecutionDuration,
       @pIcon,
       @pGroupId,
       @pContainsPrint,
       @pCustomUrl,
       @pGUID,
       @pComment,
       @pNDX,
       @pSignedDocument,
	   @pUserId
       )
  
       select @pNodeId = SCOPE_IDENTITY()
END
GO
