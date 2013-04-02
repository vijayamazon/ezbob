create table TaskedStrategies
(
ID NUMBER,
Label VARCHAR2(64),
StrategyId NUMBER,
TaskID NUMBER
);

alter table TaskedStrategies
  add constraint PK_TASKEDSTRATEGY primary key (ID);

alter table TaskedStrategies
  add constraint FK_TASKEDSTRATEGY_TASK foreign key (TaskID) 
  references StrategyTasks(ID) on delete cascade;

alter table TaskedStrategies
  add constraint FK_TASKEDSTRATEGY_STRAT foreign key (StrategyID) 
  references Strategy_Strategy(StrategyID) on delete cascade;
