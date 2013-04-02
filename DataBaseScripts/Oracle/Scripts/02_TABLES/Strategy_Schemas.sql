create table STRATEGY_SCHEMAS
(
  STRATEGYID INTEGER not null,
  BINARYDATA BLOB
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table STRATEGY_SCHEMAS
  add constraint PK_STRATEGY_SCHEMAS primary key (STRATEGYID);
