CREATE TABLE Security_Role (
       RoleId               NUMBER NOT NULL,
       Name                 VARCHAR2(255) NOT NULL,
       Description          VARCHAR2(255) NULL
);

COMMENT ON COLUMN Security_Role.RoleId IS 'primary key';
COMMENT ON COLUMN Security_Role.Name IS 'name';
COMMENT ON COLUMN Security_Role.Description IS 'Description';
CREATE UNIQUE INDEX PK_Security_Role ON Security_Role
(
       RoleId                         ASC
);


ALTER TABLE Security_Role
       ADD  ( CONSTRAINT PK_Security_Role PRIMARY KEY (RoleId) ) ;

