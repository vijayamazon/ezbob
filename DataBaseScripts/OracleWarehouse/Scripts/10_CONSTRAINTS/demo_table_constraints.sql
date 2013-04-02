alter table CreditApproval
	add constraint FK_CreditApproval_Sex
	foreign key (SEX) 
	references DictSex(ID) on delete cascade;

alter table CreditApproval
	add constraint FK_CreditApproval_MS
	foreign key (MaritalStatus) 
	references DictMaritalStatus(ID) on delete cascade;

alter table CreditApproval
	add constraint FK_CreditApproval_Chk
	foreign key (Checking) 
	references DictChecking(ID) on delete cascade;

alter table CreditApproval
	add constraint FK_CreditApproval_HO
	foreign key (HomeOwnership) 
	references DictHomeOwn(ID) on delete cascade;

alter table CreditApproval
	add constraint FK_CreditApproval_Sav
	foreign key (Savings) 
	references DictSavings(ID) on delete cascade;

alter table CreditApprovalHist 
	add constraint FK_CreditApprovalHist_HRT 
	foreign key (HRType) 
	references HistoryRecordKinds(ID) on delete cascade;
