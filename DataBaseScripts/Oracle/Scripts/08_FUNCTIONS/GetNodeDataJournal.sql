CREATE OR REPLACE FUNCTION GetNodeDataJournal
(
  pApplicationId in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
   SELECT
     Application_NodeDataSign.nodeName AS NodeName,
     1 AS IsSigned,
     Application_NodeDataSign.dateAdded AS EndExecutionDate,
     Application_NodeDataSign.outletName AS OperationType,
     Application_NodeDataSign.signedData AS SignedData,
	 Application_NodeDataSign.userName AS LastUpdateUserName,	 
	 Application_NodeDataSign.nodeId AS NodeId,
     Application_NodeDataSign.data AS Data
   FROM
     Application_NodeDataSign 
   WHERE
     Application_NodeDataSign.ApplicationId = pApplicationId;
  return l_Cursor;

END;
/
