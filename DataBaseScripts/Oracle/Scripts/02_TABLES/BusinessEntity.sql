create table BusinessEntity
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
);
alter table BusinessEntity
  add constraint PK_BusinessEntity_ID primary key (ID);