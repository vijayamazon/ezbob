create or replace function GetElementId
(
       pApplicationId in number,
	   name in varchar2
)
return sys_refcursor
AS
  l_Cursor sys_refcursor;
begin
  open l_Cursor for
	SELECT ad.DetailId
	FROM Application_Detail ad 
	INNER JOIN Application_DetailName dn ON ad.DetailNameId = dn.DetailNameId
	WHERE
		dn.name = name AND
		ad.ApplicationId = pApplicationId;
		
  return l_Cursor;
end GetElementId;
/