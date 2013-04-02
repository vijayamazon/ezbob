CREATE OR REPLACE FUNCTION GetEntityLink
(
  pEntityId IN NUMBER,
  pEntityType IN VARCHAR2,
  pIsApproved NUMBER

)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR

  SELECT LinksDoc AS DescriptionDocument
  FROM EntityLink
  WHERE EntityType = pEntityType
    AND EntityId = pEntityId
    AND SeriaId = (
      SELECT MAX(SeriaId)
      FROM EntityLink
      WHERE EntityType = pEntityType
        AND EntityId = pEntityId)
        AND (IsDeleted IS null OR IsDeleted = 0)
        AND (   (IsApproved = pIsApproved 
                 AND NOT (pIsApproved is null))
             OR (pIsApproved is null));
  return l_Cursor;

END;
/
