load data
infile 'CreditApproval.txt' "str '\r'"
into table CreditApproval
fields terminated by '#' optionally enclosed by '"'
(
ID char,
Sex char,
Age char,
AddressTime char,
MaritalStatus char,
JobTime char,
Checking char,
Savings char,
PaymentHistory char,
HomeOwnership char,
FinRatio1 char,
FinRatio2 char,
NumericWithNulls char "decode(:NumericWithNulls,'NULL','',:NumericWithNulls)"
)