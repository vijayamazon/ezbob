create or replace Procedure GetNdx
(
	pNodeId in integer,
	cur_OUT out sys_refcursor
)
AS
begin
  open cur_OUT for
	SELECT node.ndx, node.signedDocument FROM Strategy_Node node where node.NodeId = pNodeId;
end GetNdx;
/