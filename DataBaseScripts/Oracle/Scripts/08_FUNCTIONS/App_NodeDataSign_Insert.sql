CREATE OR REPLACE FUNCTION App_NodeDataSign_Insert
(
 pApplicationId NUMBER
 ,pNodeId       NUMBER
 ,pOutletName   VARCHAR2
 ,pSignedData   CLOB
 ,pData         CLOB
 ,pNodeName     VARCHAR2
 ,pUserName     VARCHAR2
) return NUMBER
AS
 l_app_NodeDataSign_id Number;
BEGIN
    Select SEQ_app_NodeDataSign.Nextval into l_app_NodeDataSign_id from dual;
    
 INSERT INTO APPLICATION_NODEDATASIGN (ID
    ,APPLICATIONID
    ,NODEID
    ,OUTLETNAME
    ,DATEADDED
    ,SIGNEDDATA
    ,DATA
    ,nodeName
    ,userName
) 
 VALUES (l_app_NodeDataSign_id
   ,pApplicationId
   ,pNodeId
   ,pOutletName
   ,sysdate
   ,pSignedData
   ,pData
   ,pNodeName
   ,pUserName);

   return l_app_NodeDataSign_id;

END;
/
