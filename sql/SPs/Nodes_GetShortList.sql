IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Nodes_GetShortList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Nodes_GetShortList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Nodes_GetShortList]
AS
BEGIN
	select security_application.name as "appname",
              strategy_node.name as "nodename",
              strategy_node.nodeid,
              strategy_node.displayname,
              strategy_node.description,
              strategy_node.nodecomment,
              security_application.applicationid,
              strategy_node.terminationdate,
              strategy_node.containsprint,
              strategy_node.executionduration,
              strategy_node.ishardreaction,
              strategy_node.Icon,
              strategy_node.Guid AS "nodeguid",
              strategy_node.Ndx
         from strategy_node,
              security_application
        where security_application.applicationid = strategy_node.applicationid
        and strategy_node.isdeleted = 0
END
GO
