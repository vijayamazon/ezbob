create or replace Procedure GetStoredParametersByAppId
  (pApplicationID number,
  cur_OUT in out sys_refcursor)
as
begin
 open cur_OUT for
  select
    Application_Detail.DetailId
   ,Application_Detail.DetailNameId
   ,Application_Detail.ParentDetailId
   ,Application_Detail.ValueStr
   ,Application_DetailName.Name
  from Application_Detail INNER JOIN Application_DetailName
         ON Application_Detail.DetailNameId = Application_DetailName.DetailNameId
   WHERE
      Application_Detail.ApplicationId = pApplicationID;
end;
/