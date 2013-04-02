CREATE TABLE Security_UserRoleRelation (
       UserId               NUMBER NOT NULL,
       RoleId               NUMBER NOT NULL
);

COMMENT ON COLUMN Security_UserRoleRelation.UserId IS 'user id';
COMMENT ON COLUMN Security_UserRoleRelation.RoleId IS 'Role Id';
CREATE UNIQUE INDEX PK_Security_UserRoleRelation ON Security_UserRoleRelation
(
       UserId                         ASC,
       RoleId                         ASC
);


ALTER TABLE Security_UserRoleRelation
       ADD  ( CONSTRAINT PK_Security_UserRoleRelation PRIMARY KEY (
              UserId, RoleId) ) ;