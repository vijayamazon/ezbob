CREATE TABLE Strategy_Node (
       NodeId               NUMBER NOT NULL,
       GroupId              NUMBER NOT NULL,
       Name                 VARCHAR2(512) NOT NULL,
       DisplayName          VARCHAR2(255) NOT NULL,
       Description          VARCHAR2(500) NULL,
       ExecutionDuration    NUMBER NULL,
       Icon                 BLOB NULL,
       IsDeleted            NUMBER DEFAULT 0 NOT NULL,
       ApplicationId        NUMBER NULL,
       IsHardReaction       NUMBER DEFAULT 0 NOT NULL,
       CONTAINSPRINT        NUMBER,
       CUSTOMURL            VARCHAR2(4000) NULL,
       STARTDATE            DATE default sysdate,
       GUID                 VARCHAR2(512) NULL,
       CreatorUserId        NUMBER NOT NULL,
       DeleterUserId        NUMBER NULL,
       NODECOMMENT          VARCHAR2(1024) NULL,
       TERMINATIONDATE      DATE NULL,
       NDX                  BLOB NULL,
       SIGNEDDOCUMENT       CLOB NULL,
       SignedDocumentDelete CLOB NULL
);

COMMENT ON COLUMN Strategy_Node.NodeId IS 'Node id';
COMMENT ON COLUMN Strategy_Node.GroupId IS '0 - Front-office; 1 - Back-office';
COMMENT ON COLUMN Strategy_Node.Name IS 'Node unique name';
COMMENT ON COLUMN Strategy_Node.DisplayName IS 'Node display name';
COMMENT ON COLUMN Strategy_Node.Description IS 'Node description';
COMMENT ON COLUMN Strategy_Node.ExecutionDuration IS 'Node execute duration (TimeSpan)';
COMMENT ON COLUMN Strategy_Node.Icon IS 'Node Icon';
COMMENT ON COLUMN Strategy_Node.IsDeleted IS '0-Active; >0 Deleted';
COMMENT ON COLUMN Strategy_Node.IsDeleted IS 'XML Schema for  Node';
COMMENT ON COLUMN Strategy_Node.NDX IS 'NDX File';
CREATE UNIQUE INDEX PK_Strategy_Node ON Strategy_Node
(
       NodeId                         ASC
);


ALTER TABLE Strategy_Node
       ADD  ( CONSTRAINT PK_Strategy_Node PRIMARY KEY (NodeId) ) ;
