create or replace function GetSecirityRolesByUserId(iUserId in number) return  SYS_REFCURSOR
AS
 l_cur sys_refcursor;
BEGIN
 OPEN l_cur FOR
  select r.RoleId, r.Name, r.Description
	from Security_Role r
	left join Security_UserRoleRelation rr on rr.RoleId = r.RoleId
	where rr.UserId = iUserId;
 return l_cur;
end GetSecirityRolesByUserId;
/