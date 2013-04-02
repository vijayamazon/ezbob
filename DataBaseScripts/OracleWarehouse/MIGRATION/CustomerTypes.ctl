load data
infile 'CustomerTypes.txt' "str '\r'"
into table CustomerTypes
fields terminated by '#' optionally enclosed by '"'
(
ID char,
DisplayName char,
Description char,
TableName char,
HistFactTableName char,
HistFactSeqName char,
HistoryTableName char,
H2HRTableName char,
IsDeleted char
)