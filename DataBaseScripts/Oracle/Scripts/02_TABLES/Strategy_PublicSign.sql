-- Create table
create table Strategy_PublicSign
(
  ID               NUMBER not null,
  STRATEGYPUBLICID NUMBER not null,
  StrategyId       NUMBER not null,
  CREATIONDATE     DATE   not null,
  DATA             CLOB   not null,
  ACTION           VARCHAR2(7) not null,
  SIGNEDDOCUMENT   CLOB   not null,
  USERID           NUMBER not null,
  AllData		   CLOB   not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table Strategy_PublicSign
  add constraint PK_Strategy_PublicSign primary key (ID);