/*
Edit by Kirill Sorudeykin
Date: Mar 26, 2008
*/
CREATE OR REPLACE procedure UpdateRole
  (
    pRoleId in Number,
    pRoleName in Varchar2,
    pRoleDescr in Varchar2,
    pUsersIds in Varchar2
  )
as
 l_new_role_id Number;
 rrr Number;
begin
  l_new_role_id := pRoleId;

  if l_new_Role_id > 0 then
   UPDATE Security_Role
      SET Name = pRoleName,
          Description = pRoleDescr WHERE RoleId = pRoleId;

   DELETE FROM Security_UserRoleRelation WHERE RoleId = pRoleId;
  else
    insert_security_role(pRoleName, pRoleDescr, l_new_role_id);
  end if;
  
  select count(column_value) into rrr from TABLE(str2tbl(pUsersIds));
  
  if rrr>0 then  
  FOR lUsrRoleRel in (Select x.column_value, l_new_role_id from TABLE(str2tbl(pUsersIds)) x)
  LOOP
      INSERT INTO Security_UserRoleRelation
      VALUES(lUsrRoleRel.Column_Value, lUsrRoleRel.l_New_Role_Id);
  END LOOP;
  end if;

exception when others then
  raise;

end;
/