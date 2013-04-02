execute immediate 'create table BusinessEntity
(
  ID                   NUMBER not null,
  NAME                 VARCHAR2(25),
  DESCRIPTION          VARCHAR2(1024),
  Review               VARCHAR2(1024),
  ISDELETED            NUMBER,
  CREATIONDATE         TIMESTAMP(6),
  TERMINATIONDATE      TIMESTAMP(6),
  USERID               NUMBER,
  DOCUMENT             CLOB,
  SignedDocument       CLOB,
  SignedDocumentDelete CLOB,
  ItemVersion            VARCHAR2(50) NOT NULL
)';

execute immediate 'alter table BusinessEntity
  add constraint PK_BusinessEntity_ID primary key (ID)';

execute immediate 'create table BusinessEntity_NodeRel
(
  ID               NUMBER not null,
  NodeId           NUMBER not null,
  BusinessEntityId NUMBER not null
)';

execute immediate 'alter table BusinessEntity_NodeRel
  add constraint PK_BusinessEntity_NodeRel primary key (ID)';


execute immediate 'create table BusinessEntity_StrategyRel
(
  ID               NUMBER not null,
  StrategyId       NUMBER not null,
  BusinessEntityId NUMBER not null
)';

execute immediate 'alter table BusinessEntity_StrategyRel
  add constraint PK_BusinessEntity_StrategyRel primary key (ID)';


