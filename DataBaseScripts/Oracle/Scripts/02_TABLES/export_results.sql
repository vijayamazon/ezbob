-- Create table
create table EXPORT_RESULTS
(
  ID               NUMBER not null,
  FILENAME         VARCHAR2(3000),
  BINARYBODY       BLOB,
  FILETYPE         NUMBER,
  CREATIONDATE     DATE default sysdate,
  SOURCETEMPLATEID NUMBER,
  APPLICATIONID    NUMBER,
  STATUS           NUMBER,
  STATUSMODE       NUMBER,
  NODENAME         VARCHAR2(500) not null,
  SignedDocumentId NUMBER NULL
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table EXPORT_RESULTS
  add constraint PK_RESULTID primary key (ID);
-- Add comments to the columns 
comment on column EXPORT_RESULTS.STATUSMODE
  is 'NULL - not readed, 1- readed';