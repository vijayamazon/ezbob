IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNodeFilesForRestoreAndSave]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNodeFilesForRestoreAndSave]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNodeFilesForRestoreAndSave]
(
       @pNodeId int
)
AS
BEGIN
     select sn.NodeId,
            sn.Name,
            sn.NDX
     from Strategy_node sn
     where sn.nodeid = @pNodeId
END
GO
