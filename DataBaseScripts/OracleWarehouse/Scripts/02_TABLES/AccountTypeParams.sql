create table AccountTypeParams
(
ID NUMBER not null ,
AccountTypeID  NUMBER not null ,
FieldName VARCHAR2(30) not null, 
FieldType VARCHAR2(30) not null, 
IsDeleted VARCHAR2(1),
DictionaryID  NUMBER
);
