CREATE OR REPLACE FUNCTION Strategy_GetPublishNamesCount
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       SELECT p.name, 
            (SELECT COUNT(strategyId) FROM Strategy_Publicrel 
            WHERE publicId = p.publicnameid) STRATEGYCOUNT, p.publicnameid, p.isstopped
       FROM Strategy_PublicName p
       WHERE  (IsDeleted is null or IsDeleted = 0);
  return l_Cursor;

END;
/

