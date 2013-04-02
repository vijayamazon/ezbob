-- Create table
create table StrategyAccountRel
(
  AccountId NUMBER not null,
  StrategyId  NUMBER not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table StrategyAccountRel
  add constraint PK_StrategyAccountRel primary key (AccountId, StrategyId);