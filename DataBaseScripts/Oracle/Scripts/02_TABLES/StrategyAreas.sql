create table StrategyAreas
(
ID NUMBER,
Name VARCHAR2(64),
Description VARCHAR2(1024)
);
alter table StrategyAreas
  add constraint PK_STRATEGYAREA primary key (ID);

alter table StrategyAreas
  add constraint IX_STRATEGYAREA UNIQUE (Name);