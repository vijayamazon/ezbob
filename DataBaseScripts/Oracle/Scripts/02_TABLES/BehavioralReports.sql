-- Create table
create table BehavioralReports
(
  ID           	NUMBER not null,
  STRATEGYID   	NUMBER,
  NAME         	VARCHAR2(1024),
  TYPEID       	NUMBER,
  PATH	       	VARCHAR2(2048),
  CREATIONDATE	DATE,
  TESTRUN       NUMBER,
  ISNOTREAD    	NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table BehavioralReports
  add constraint PK_BehavioralReports primary key (ID);
    

