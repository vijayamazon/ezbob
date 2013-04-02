-- Create table
create table DATASOURCE_SOURCES
(
  ID                   NUMBER not null,
  NAME                 VARCHAR2(255) NOT NULL,
  Description          VARCHAR2(500) NULL,
  TYPE                 VARCHAR2(20) NOT NULL,
  DOCUMENT             CLOB NOT NULL,
  SIGNEDDOCUMENT       CLOB NULL,
  USERID               NUMBER,
  ISDELETED            NUMBER,
  CREATIONDATE         DATE,
  TERMINATIONDATE      DATE,
  DISPLAYNAME          VARCHAR2(255) NOT NULL,
  SignedDocumentDelete CLOB NULL
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table DATASOURCE_SOURCES
  add constraint PK_DATASOURCE_SOURCES primary key (ID);
  
alter table DATASOURCE_SOURCES
  add constraint IX_DATASOURCE_SOURCES unique (NAME, ISDELETED);
  

