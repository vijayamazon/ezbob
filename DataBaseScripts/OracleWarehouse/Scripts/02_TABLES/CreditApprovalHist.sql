create table CreditApprovalHist
(
id number, 
HRDate date, 
HRLoadDate date, 
HRName VARCHAR2(30), 
HRType number, 
HRecordAlias number
);

alter table CreditApprovalHist 
	add constraint PK_CreditApprovalHist 
	primary key (ID);
