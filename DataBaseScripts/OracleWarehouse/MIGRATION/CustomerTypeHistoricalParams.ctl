load data
infile 'CustomerTypeHistoricalParams.txt' "str '\r'"
into table CustomerTypeHistoricalParams
fields terminated by '#' optionally enclosed by '"'
(
ID char,
CustomerTypeID char,
FieldName char,
FieldType char,
DICTIONARYID char "decode(:DICTIONARYID,'NULL','',:DICTIONARYID)"
)