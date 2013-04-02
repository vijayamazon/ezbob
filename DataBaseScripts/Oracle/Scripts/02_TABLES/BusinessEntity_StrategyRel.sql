-- Create table
create table BusinessEntity_StrategyRel
(
  ID               NUMBER not null,
  StrategyId       NUMBER not null,
  BusinessEntityId NUMBER not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table BusinessEntity_StrategyRel
  add constraint PK_BusinessEntity_StrategyRel primary key (ID);