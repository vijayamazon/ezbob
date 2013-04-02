CREATE OR REPLACE FUNCTION GetNodeFilesForRestoreAndSave
(
	pNodeId in number
) 
return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
     select sn.NodeId,
			sn.Name,
            sn.NDX
     from Strategy_node sn
     where sn.nodeid = pNodeId;
  return lcur;
end GetNodeFilesForRestoreAndSave;
/
