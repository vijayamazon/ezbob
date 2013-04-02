load data
infile 'MenuItem.txt' "str '\r'"
into table MenuItem
fields terminated by '#' optionally enclosed by '"'
(Id	char,
Caption		char,
Url			char,
SecAppId	char,
Position	char,
FilterId	char,
Filter		char(4000)
 )

