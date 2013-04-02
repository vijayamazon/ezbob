load data
infile 'Strategy_ParameterType.txt' "str '\r'"
into table Strategy_ParameterType
fields terminated by '#' optionally enclosed by '"'
(PARAMTYPEID char,
NAME char,
DESCRIPTION char "decode(:DESCRIPTION,'NULL','',:DESCRIPTION)"
 )