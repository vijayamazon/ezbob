create table CreditApproval
(
ID NUMBER not null ,
Sex NUMBER,
Age NUMBER,
AddressTime NUMBER,
MaritalStatus NUMBER,
JobTime NUMBER,
Checking NUMBER,
Savings NUMBER,
PaymentHistory NUMBER,
HomeOwnership NUMBER,
FinRatio1 NUMBER,
FinRatio2 NUMBER,
NumericWithNulls NUMBER
);

alter table CreditApproval add constraint PK_CreditApproval primary key (ID);

