CREATE OR REPLACE FUNCTION AddLink
(
  pEntityId   NUMBER,
  pSeriaId    NUMBER,
  pUserId     NUMBER,
  pEntityType VARCHAR2,
  pLinksDoc   CLOB,
  pSignedDoc  CLOB,
  pIsApproved NUMBER
)
RETURN NUMBER
as
  l_id NUMBER;
begin
  select SEQ_EntityLink.nextval into l_id from dual;

  INSERT INTO EntityLink
     (ID
    , SeriaId
    , EntityType
    , EntityId
    , UserId
    , LinksDoc
    , SignedDoc
    , IsApproved)
  VALUES
     (l_id
    , pSeriaId
    , pEntityType
    , pEntityId
    , pUserId
    , pLinksDoc
    , pSignedDoc
    , pIsApproved);

return l_id;

end;
/