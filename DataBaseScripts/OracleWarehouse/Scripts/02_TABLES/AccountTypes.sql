create table AccountTypes
(
ID NUMBER not null , 
DisplayName VARCHAR2(255) not null, 
Description VARCHAR2(1024), 
TableName VARCHAR2(30) not null, 
HistFactTableName VARCHAR2(30) not null, 
HistFactSeqName VARCHAR2(30) not null,
HistoryTableName VARCHAR2(30) not null, 
H2HRTableName VARCHAR2(30) not null, 
CustomerTypeId number not null ,
CreationTime date, 
UserID NUMBER, 
IsDeleted VARCHAR2(1)
);
