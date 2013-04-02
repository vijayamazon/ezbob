create table SCORINGMODEL
(
  ID                   NUMBER not null,
  GUID                 VARCHAR2(250),
  DISPLAYNAME          VARCHAR2(1024),
  USERID               NUMBER,
  MODELTYPENAME        VARCHAR2(250),
  CREATIONDATE         TIMESTAMP(6),
  ISDELETED            NUMBER,
  TERMINATIONDATE      TIMESTAMP(6),
  DESCRIPTION          VARCHAR2(1024),
  CUTOFFPOINT          NUMBER,
  ALLOWWEIGHTSEDIT     NUMBER,
  ALLOWSAVERESULTS     NUMBER,
  PMMLFILE             CLOB,
  SignedDocument       CLOB,
  SignedDocumentDelete CLOB
);
alter table SCORINGMODEL
  add constraint PK_SCORINGMODEL_ID primary key (ID);