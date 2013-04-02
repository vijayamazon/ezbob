-- Create table
create table DataDestinationParams
(
  ID		NUMBER not null,
  NAME		VARCHAR2(1024),
  TYPE		VARCHAR2(1024),
  DESCRIPTION	VARCHAR2(1024),
  CONSTRAINT 	VARCHAR2(1024),
  DICTIONARYID	NUMBER,
  DESTINATIONID	NUMBER
);

-- Create/Recreate primary, unique and foreign key constraints 
alter table DataDestinationParams
  add constraint PK_DATADEST_PARAMS primary key (ID);

alter table DataDestinationParams
  add constraint IX_DATADEST_PARAMS unique (NAME, DESTINATIONID);