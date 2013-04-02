create table CustomerTypeHistoricalParams
(
ID NUMBER not null ,
CustomerTypeID NUMBER not null ,
FieldName VARCHAR2(30) not null, 
FieldType VARCHAR2(30) not null, 
IsDeleted VARCHAR2(1),
DictionaryId number
);
