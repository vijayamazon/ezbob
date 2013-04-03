IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNdx]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNdx]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNdx] @pNodeId  int
AS	
BEGIN
	SELECT node.[ndx], node.[signedDocument] FROM Strategy_Node node where node.NodeId = @pNodeId
    RETURN    
END
GO
