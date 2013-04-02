
alter table Dictionaries
  add constraint IX_Dictionaries unique (DisplayName);

alter table Dictionaries
  add constraint PK_Dictionaries primary key (id);

alter table DictionaryParams
  add constraint PK_DictionaryParams primary key (id);

alter table DictionaryParams
  add constraint FK_DictionaryParams foreign key (DictionaryID)
  references Dictionaries (ID) on delete cascade ;
  
alter table DictionaryParams
  add constraint FK_DictionaryParams_MastDict foreign key (MasterDictionaryID)
  references Dictionaries (ID) on delete cascade ;

alter table DICTIONARYPARAMS
  add constraint IX_DICTIONARYPARAMS unique (DISPLAYNAME, DICTIONARYID);

alter table CustomerTypes
  add constraint IX_CustomerTypes unique (DisplayName);

alter table CustomerTypes
  add constraint PK_CustomerTypes primary key (id);

alter table CustomerTypeParams
  add constraint PK_CustomerTypeParams primary key (id);

alter table CustomerTypeParams
  add constraint FK_CustomerTypeParams foreign key (CustomerTypeID)
  references CustomerTypes (ID) on delete cascade ;
  
alter table CustomerTypeParams
  add constraint FK_CustomerTypeParams_Dict foreign key (DictionaryId)
  references Dictionaries (ID) on delete cascade ;  


alter table AccountTypes
  add constraint IX_AccountTypes unique (DisplayName);

alter table AccountTypes
  add constraint PK_AccountTypes primary key (id);

alter table AccountTypes
  add constraint FK_AccountTypes foreign key (CustomerTypeID)
  references CustomerTypes (ID) on delete cascade ;


alter table AccountTypeParams
  add constraint PK_AccountTypeParams primary key (id);

alter table AccountTypeParams
  add constraint FK_AccountTypeParams foreign key (AccountTypeID)
  references AccountTypes (ID) on delete cascade ;
  
alter table AccountTypeParams
  add constraint FK_AccountTypeParams_Dict foreign key (DictionaryID)
  references Dictionaries (ID) on delete cascade ;


alter table DataSources
  add constraint FK_DS_CUS_ID foreign key (REF_CUSTYPEID) 
  references CustomerTypes (ID) on delete cascade;

alter table DataSources
  add constraint FK_DS_ACC_ID foreign key (REF_ACCTYPEID) 
  references AccountTypes (ID) on delete cascade;

  
alter table DataSourceParams
  add constraint FK_DSPARAM_DS foreign key (DATASOURCEID) 
  references DataSources (ID) on delete cascade;

alter table DataSourceParams
  add constraint FK_DSPARAM_DIC foreign key (DICTIONARYID) 
  references Dictionaries (ID) on delete cascade;
  
alter table DataDestinations
  add constraint FK_DD_DSID foreign key (DATASOURCEID) 
	references DataSources (ID) on delete cascade;

alter table DataDestinationParams
  add constraint FK_DDPARAM_DD foreign key (DESTINATIONID) 
	references DataDestinations (ID) on delete cascade;

alter table DataDestinationParams
  add constraint FK_DDPARAM_DIC foreign key (DICTIONARYID) 
  references Dictionaries (ID) on delete cascade;
	