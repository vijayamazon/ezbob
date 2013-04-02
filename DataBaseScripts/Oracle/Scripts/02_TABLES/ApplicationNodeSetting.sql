CREATE TABLE Application_NodeSetting (
       Id                   NUMBER NOT NULL,
       NodeId               NUMBER,
       ApplicationId        NUMBER NOT NULL,
       NODEPOSTFIX          VARCHAR2(1000 BYTE) NOT NULL,
       Name                 VARCHAR2(150) NOT NULL,
       Value                NUMBER NULL
);

COMMENT ON COLUMN Application_NodeSetting.NodeId IS 'Node id';
COMMENT ON COLUMN Application_NodeSetting.ApplicationId IS 'Application identifier';
COMMENT ON COLUMN Application_NodeSetting.Name IS 'Setting name';
COMMENT ON COLUMN Application_NodeSetting.Value IS 'Setting value';

CREATE UNIQUE INDEX PK_Application_NodeSetting ON Application_NodeSetting
(
       Id           ASC
);


ALTER TABLE Application_NodeSetting
       ADD  ( CONSTRAINT PK_Application_NodeSetting PRIMARY KEY (Id) ) ;
