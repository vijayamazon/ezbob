insert into Security_Role(Name,Description)
values('LegalDocsReview','Allow user to add draft and insert to DB')

insert into Security_Role(Name,Description)
values('LegalDocsAdmin','Allow user to edit and approve legal docs')

insert into Security_Permission(id,Name, Description)
values(33,'LegalDocsReview','LegalDocsReview')

insert into Security_Permission(id,Name, Description)
values(34,'LegalDocsApprove','LegalDocsApprove')

insert into Security_RolePermissionRel (RoleId,PermissionId)
values(41,33)

insert into Security_RolePermissionRel (RoleId,PermissionId)
values(42,33)

insert into Security_RolePermissionRel (RoleId,PermissionId)
values(42,34)

select * from Security_User where username like 'dora+@ezbob.com'

insert into Security_UserRoleRelation (UserId, RoleId)
values(170,42)

insert into Security_UserRoleRelation (UserId, RoleId)
values(413,41)

