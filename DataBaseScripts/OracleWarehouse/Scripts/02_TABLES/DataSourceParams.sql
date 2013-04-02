-- Create table
create table DataSourceParams
(
  ID		NUMBER not null,
  NAME		VARCHAR2(255),
  TYPE		VARCHAR2(255),
  CONSTRAINT	VARCHAR2(1024),
  DESCRIPTION	VARCHAR2(1024),
  ISHISTORICAL  NUMBER,
  ISIDENTITY	NUMBER,
  DICTIONARYID	NUMBER,
  DATASOURCEID	NUMBER
);

-- Create/Recreate primary, unique and foreign key constraints 
alter table DataSourceParams
  add constraint PK_DATASOURCEPARAMS primary key (ID);

alter table DataSourceParams
  add constraint IX_DATASOURCEPARAMS unique (NAME, DATASOURCEID);
