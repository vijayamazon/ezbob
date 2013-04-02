create table TaskedStrategyParams
(
ID NUMBER,
TSID NUMBER,
Name VARCHAR2(256),
DisplayName VARCHAR2(256),
Description VARCHAR2(1024),
TypeName VARCHAR2(64),
InitialValue VARCHAR2(256),
ConstraintString VARCHAR2(1024)
);

alter table TaskedStrategyParams
  add constraint PK_TSPARAM primary key (ID);

alter table TaskedStrategyParams
  add constraint FK_TSPARAM_TS foreign key (TSID) 
  references TaskedStrategies(ID) on delete cascade;