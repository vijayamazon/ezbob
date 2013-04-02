-- Create table
create table DATASOURCE_STRATEGYREL
(
  ID              NUMBER not null,
  DATASOURCENAME  VARCHAR(1024),
  STRATEGYID      NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table DATASOURCE_STRATEGYREL
  add constraint PK_DATASOURCE_STRATEGYREL primary key (ID);