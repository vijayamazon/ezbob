--Approves link using AppId
CREATE OR REPLACE PROCEDURE ApproveLinksBySourceId
  (
    pLinkedFrom IN NUMBER,
    pEntityType IN VARCHAR2,
    pIsApproved IN NUMBER
   )
AS
BEGIN
 UPDATE EntityLink
   SET IsApproved = pIsApproved
 WHERE EntityId = pLinkedFrom
   AND EntityType = pEntityType
   AND (EntityLink.IsDeleted = 0 OR EntityLink.IsDeleted IS NULL);
END ApproveLinksBySourceId;
/