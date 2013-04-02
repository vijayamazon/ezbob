-- Create table
create table DATADESTINATIONS
(
  ID		NUMBER not null,
  NAME		VARCHAR2(1024),
  DESCRIPTION	VARCHAR2(1024),
  IDENTITYFIELD	VARCHAR2(30),
  REFERENCEFIELD VARCHAR2(30),
  FACTS_TABLE	VARCHAR2(30),
  FACTS_SEQ	VARCHAR2(30),
  H2F_TABLE	VARCHAR2(30),
  HIST_TABLE	VARCHAR2(30),
  DATASOURCEID	NUMBER,
  ISDELETED	NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table DATADESTINATIONS
  add constraint PK_DATADEST primary key (ID);
  
alter table DATADESTINATIONS
  add constraint IX_DATADEST unique (NAME, ISDELETED);
  

