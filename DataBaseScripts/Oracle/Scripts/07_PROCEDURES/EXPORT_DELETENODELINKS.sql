CREATE OR REPLACE PROCEDURE Export_DeleteNodeLinks
  (
    pNodeId in number
  )
AS
BEGIN
   delete from export_templatenoderel 
   where nodeid = pNodeId;
END;
/

