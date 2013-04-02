 alter table DictionaryParams enable validate constraint  FK_DictionaryParams ; 
 alter table DictionaryParams enable validate constraint  FK_DictionaryParams_MastDict ; 
 alter table CustomerTypeParams enable validate constraint  FK_CustomerTypeParams ; 
 alter table CustomerTypeParams enable validate constraint  FK_CustomerTypeParams_Dict ; 
 alter table AccountTypes enable validate constraint  FK_AccountTypes ; 
 alter table AccountTypeParams enable validate constraint  FK_AccountTypeParams ; 
 alter table AccountTypeParams enable validate constraint  FK_AccountTypeParams_Dict ; 
 alter table HISTORYRECORDKINDS enable validate constraint PK_HistoryRecordKinds ;

alter table DataSources
  enable validate constraint FK_DS_CUS_ID;

alter table DataSources
  enable validate constraint FK_DS_ACC_ID;


alter table DataSourceParams
  enable validate constraint FK_DSPARAM_DS;

alter table DataSourceParams
  enable validate constraint FK_DSPARAM_DIC;


alter table DataDestinations
  enable validate constraint FK_DD_DSID;

alter table DataDestinationParams
  enable validate constraint FK_DDPARAM_DD;

alter table DataDestinationParams
  enable validate constraint FK_DDPARAM_DIC;




alter table CreditApproval
	enable validate constraint FK_CreditApproval_Sex;

alter table CreditApproval
	enable validate constraint FK_CreditApproval_MS;

alter table CreditApproval
	enable validate constraint FK_CreditApproval_Chk;

alter table CreditApproval
	enable validate constraint FK_CreditApproval_HO;

alter table CreditApproval
	enable validate constraint FK_CreditApproval_Sav;

alter table CreditApprovalHist 
	enable validate constraint FK_CreditApprovalHist_HRT ;



quit;

/