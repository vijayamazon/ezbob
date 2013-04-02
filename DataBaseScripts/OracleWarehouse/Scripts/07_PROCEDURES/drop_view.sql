CREATE OR REPLACE PROCEDURE drop_view
(pViewName IN VARCHAR2)
AS
 cnt NUMBER;
BEGIN
   select count(*) into cnt from user_views where upper(view_name) = upper(pViewName);
   if cnt > 0 then
      execute immediate 
      'drop view ' || pViewName;
   end if;
END drop_view;
/

