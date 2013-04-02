CREATE OR REPLACE PROCEDURE Strategy_NodeInsert
   (
     pName in varchar2,
     pDisplayName in varchar2,
     pDescription in varchar2,
     pExecutionDuration in Number,
     pIcon in blob,
     pGroupId in number,
     pContainsPrint in number,
     pNodeId out Number,
     pCustomUrl IN VARCHAR2,
     pGUID in varchar2,
     pComment in varchar2,
     pNDX in blob,
     pSignedDocument in clob,
	 pUserId in int
   )
AS
BEGIN
     -- Terminate current nodes
     update Strategy_Node set TerminationDate  = sysdate
     where upper(Name) = upper(pName) and TerminationDate is null;

     Select SEQ_STRATEGY_NODE.NEXTVAL into pNodeId from dual;

     insert into Strategy_Node
     (
       NodeId,
       Name,
       DisplayName,
       Description,
       ExecutionDuration,
       Icon,
       GroupId,
       ContainsPrint,
       customurl,
       guid,
       nodecomment, 
       NDX,
       SignedDocument,
       CreatorUserId
     )
     values
     (
       pNodeId,
       pName,
       pDisplayName,
       pDescription,
       pExecutionDuration,
       pIcon,
       pGroupId,
       pContainsPrint,
       pCustomUrl,
       pGUID,
       pComment,
       pNDX,
       pSignedDocument,
	   pUserId
     );
--     commit;
 exception when others then
--   rollback;
   raise;
END;
/

