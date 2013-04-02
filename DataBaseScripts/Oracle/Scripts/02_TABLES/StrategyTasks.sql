create table StrategyTasks
(
ID NUMBER,
Name VARCHAR2(64),
Description VARCHAR2(1024),
AreaID NUMBER
);

alter table StrategyTasks
  add constraint PK_STRATEGYTASK primary key (ID);

alter table StrategyTasks
  add constraint FK_STRATEGYTASK_AREA foreign key (AreaID) 
  references StrategyAreas(ID) on delete cascade;
