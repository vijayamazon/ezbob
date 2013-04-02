load data
infile 'CreditApprovalHist.txt' "str '\r'"
into table CreditApprovalHist
fields terminated by '#' optionally enclosed by '"'
(
ID char,
HRDate DATE "YYYY-MM-DD HH24:MI:SS",
HRLoadDate DATE "YYYY-MM-DD HH24:MI:SS",
HRName char,
HRType char,
HRecordAlias char "decode(:HRecordAlias,'NULL','',:HRecordAlias)"
)