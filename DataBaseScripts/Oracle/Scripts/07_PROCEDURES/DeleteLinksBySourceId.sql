CREATE OR REPLACE PROCEDURE DeleteLinksBySourceId
  (
    pLinkedFrom IN NUMBER,
    pEntityType IN VARCHAR2
  )
AS
BEGIN
 UPDATE EntityLink
   SET IsDeleted = 1
 WHERE EntityId = pLinkedFrom OR SeriaId = pLinkedFrom
   AND EntityType = pEntityType;
END DeleteLinksBySourceId;
/