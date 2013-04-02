CREATE TABLE Strategy_NodeGroup (
       NodeGroupId          SMALLINT NOT NULL,
       Name                 VARCHAR2(50) NOT NULL
);

COMMENT ON COLUMN Strategy_NodeGroup.NodeGroupId IS 'Primary key';
COMMENT ON COLUMN Strategy_NodeGroup.Name IS 'Group Name';
CREATE UNIQUE INDEX PK_Strategy_NodeGroup ON Strategy_NodeGroup
(
       NodeGroupId                    ASC
);


ALTER TABLE Strategy_NodeGroup
       ADD  ( CONSTRAINT PK_Strategy_NodeGroup PRIMARY KEY (
              NodeGroupId) ) ;


