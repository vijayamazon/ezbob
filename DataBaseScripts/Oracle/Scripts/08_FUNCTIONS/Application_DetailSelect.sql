CREATE OR REPLACE FUNCTION Application_DetailSelect
 (
    pApplicationId IN NUMBER
 ) RETURN sys_refcursor
AS
  l_cur sys_refcursor;
BEGIN
-- Description:  Select all details for given ApplicationID
   open l_cur for
      SELECT ad.DetailId     as DetailId
         ,ad.ParentDetailId  as ParentDetailId
         ,ad.ValueStr        as ValueStr
         ,ad.ValueNum        as ValueNum
         ,ad.ValueDateTime   as ValueDateTime
         ,ad.IsBinary        as IsBinary
         ,adn.Name           as Name
      FROM Application_Detail ad 
          JOIN Application_DetailName adn 
             ON adn.DetailNameId(+) = ad.DetailNameId 
      WHERE ApplicationId = pApplicationId 
      ORDER BY ad.ParentDetailID;
  RETURN l_cur;
  
END Application_DetailSelect;
/
