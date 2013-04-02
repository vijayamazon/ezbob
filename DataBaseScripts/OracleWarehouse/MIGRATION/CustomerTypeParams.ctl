load data
infile 'CustomerTypeParams.txt' "str '\r'"
into table CustomerTypeParams
fields terminated by '#' optionally enclosed by '"'
(
ID char,
CustomerTypeID char,
FieldName char,
FieldType char,
DictionaryId char "decode(:DictionaryId,'NULL','',:DictionaryId)"
)