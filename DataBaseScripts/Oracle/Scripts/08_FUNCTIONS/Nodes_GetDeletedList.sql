CREATE OR REPLACE FUNCTION Nodes_GetDeletedList
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
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
        and strategy_node.isdeleted != 0;
  return l_Cursor;

END;
/
