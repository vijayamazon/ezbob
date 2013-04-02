create or replace function App_NodeDataSign_Select
(pSignedDocumentId number)

 return sys_refcursor

 AS

  l_cur sys_refcursor;
  l_id number;


BEGIN

    open l_cur for

    SELECT
        signedData
        ,data
        ,outletName
        ,nodeId
        ,userName
    FROM  Application_NodeDataSign
    WHERE id = pSignedDocumentId;

    return l_cur;

END;
/