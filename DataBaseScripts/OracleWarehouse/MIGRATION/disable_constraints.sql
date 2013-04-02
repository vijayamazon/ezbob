 alter table DictionaryParams disable constraint  FK_DictionaryParams ; 
 alter table DictionaryParams disable constraint  FK_DictionaryParams_MastDict ; 
 alter table CustomerTypeParams disable constraint  FK_CustomerTypeParams ; 
 alter table CustomerTypeParams disable constraint  FK_CustomerTypeParams_Dict ; 
 alter table AccountTypes disable constraint  FK_AccountTypes ; 
 alter table AccountTypeParams disable constraint  FK_AccountTypeParams ; 
 alter table AccountTypeParams disable constraint  FK_AccountTypeParams_Dict ; 
 alter table HISTORYRECORDKINDS disable constraint PK_HistoryRecordKinds ;

alter table DataSources
  disable  constraint FK_DS_CUS_ID;

alter table DataSources
  disable  constraint FK_DS_ACC_ID;

alter table DataSourceParams
  disable constraint FK_DSPARAM_DS;

alter table DataSourceParams
  disable constraint FK_DSPARAM_DIC;

alter table DataDestinations
  disable constraint FK_DD_DSID;

alter table DataDestinationParams
  disable constraint FK_DDPARAM_DD;

alter table DataDestinationParams
  disable constraint FK_DDPARAM_DIC;

alter table CreditApproval
	disable  constraint FK_CreditApproval_Sex;

alter table CreditApproval
	disable  constraint FK_CreditApproval_MS;

alter table CreditApproval
	disable  constraint FK_CreditApproval_Chk;

alter table CreditApproval
	disable  constraint FK_CreditApproval_HO;

alter table CreditApproval
	disable  constraint FK_CreditApproval_Sav;

alter table CreditApprovalHist 
	disable  constraint FK_CreditApprovalHist_HRT ;

quit;

/