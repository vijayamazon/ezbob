load data
infile 'CA_H2HR.txt' "str '\r'"
into table CA_H2HR
fields terminated by '#' optionally enclosed by '"'
(
HISTORICALID char,
HISTORYRECORDID char
)