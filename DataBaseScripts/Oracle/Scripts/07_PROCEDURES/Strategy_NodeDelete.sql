create or replace procedure Strategy_NodeDelete(pNodeName in varchar2, pSignedDocument in clob,pUserId in int)
AS
BEGIN
  update Strategy_Node
  set
    IsDeleted = nodeid,
    TerminationDate  = sysdate,
    SignedDocumentDelete = pSignedDocument,
    DeleterUserId = pUserId
  where upper(name) = upper(pNodeName);
END;
/
