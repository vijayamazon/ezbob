IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNodeDataJournal]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNodeDataJournal]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNodeDataJournal] 
	(@pApplicationId bigint)
AS
BEGIN
	SELECT
    Application_NodeDataSign.nodeName AS NodeName,
    1 AS IsSigned,
    Application_NodeDataSign.dateAdded AS EndExecutionDate,
    Application_NodeDataSign.outletName AS OperationType,
    Application_NodeDataSign.signedData AS SignedData,
    Application_NodeDataSign.userName AS LastUpdateUserName,	 
    Security_User.FullName AS LastUpdateUserFullName,	 
    Application_NodeDataSign.nodeId AS NodeId,
    Application_NodeDataSign.data AS Data,
    Application_NodeDataSign.UserName AS UserName
   FROM
    Application_NodeDataSign, Security_User
   WHERE
    Application_NodeDataSign.ApplicationId = @pApplicationId
	and Application_NodeDataSign.userName = Security_User.UserName
END
GO
