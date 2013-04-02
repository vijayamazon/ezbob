load data
infile 'HistoryRecordKinds.txt' "str '\r'"
into table HistoryRecordKinds
fields terminated by '#' optionally enclosed by '"'
(ID char,
DISPLAYNAME char,
ISALIAS char
)