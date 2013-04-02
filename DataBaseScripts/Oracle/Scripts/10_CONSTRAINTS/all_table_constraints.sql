ALTER TABLE StrategyEngine_ExecutionState
       ADD  ( CONSTRAINT PK_StrEngine_ExecState PRIMARY KEY (
              Id) ) ;


			  
ALTER TABLE Application_Application
       ADD  ( CONSTRAINT FK_App_App_SecUsr_LckdByUsrId
              FOREIGN KEY (LockedByUserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Application_Application
       ADD  ( CONSTRAINT FK_App_App_SecUsr_CreaUsrId
              FOREIGN KEY (CreatorUserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Application_Application
       ADD  ( CONSTRAINT FK_App_App_StrStr_StrId
              FOREIGN KEY (StrategyId)
                             REFERENCES Strategy_Strategy ) ;


ALTER TABLE Application_Attachment
       ADD  ( CONSTRAINT FK_App_Att_Appl_Det_detId
              FOREIGN KEY (DetailId)
                             REFERENCES Application_Detail
                             ON DELETE CASCADE ) ;


ALTER TABLE Application_Detail
       ADD  ( CONSTRAINT FK_App_Dtl_ADN_DetNmId
              FOREIGN KEY (DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Detail
       ADD  ( CONSTRAINT FK_App_Dtl_App_App_appid
              FOREIGN KEY (ApplicationId)
                             REFERENCES Application_Application ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName9
              FOREIGN KEY (Param9DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName8
              FOREIGN KEY (Param8DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName7
              FOREIGN KEY (Param7DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName6
              FOREIGN KEY (Param6DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName5
              FOREIGN KEY (Param5DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName4
              FOREIGN KEY (Param4DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName3
              FOREIGN KEY (Param3DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName2
              FOREIGN KEY (Param2DetailNameId)
                             REFERENCES Application_DetailName ) ;


ALTER TABLE Application_Setting
       ADD  ( CONSTRAINT FK_App_Setting_App_dtlName1
              FOREIGN KEY (Param1DetailNameId)
                             REFERENCES Application_DetailName ) ;

ALTER TABLE Security_Session
       ADD  ( CONSTRAINT FK_Secty_Sess_Secty_UsrId
              FOREIGN KEY (UserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Security_Session
       ADD  ( CONSTRAINT FK_Secty_Sessn_Secty_appId
              FOREIGN KEY (AppId)
                             REFERENCES Security_Application ) ;


ALTER TABLE Security_User
       ADD  ( CONSTRAINT FK_Secty_Usr_Sec_Usr_delusrId
              FOREIGN KEY (DeleteUserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Security_User
       ADD  ( CONSTRAINT FKSecurity_User_createdUsrId
              FOREIGN KEY (CreateUserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Security_UserRoleRelation
       ADD  ( CONSTRAINT FK_Sec_User_UserId
              FOREIGN KEY (UserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Security_UserRoleRelation
       ADD  ( CONSTRAINT FK_Security_Role_RoleId
              FOREIGN KEY (RoleId)
                             REFERENCES Security_Role
                             ON DELETE CASCADE ) ;


ALTER TABLE Strategy_Node
       ADD  ( CONSTRAINT FK_Security_Appl_appId
              FOREIGN KEY (ApplicationId)
                             REFERENCES Security_Application ) ;


ALTER TABLE Strategy_Node
       ADD  ( CONSTRAINT FK_Strategy_NodeGroup_groupid
              FOREIGN KEY (GroupId)
                             REFERENCES Strategy_NodeGroup ) ;


ALTER TABLE Strategy_NodeStrategyRel
       ADD  ( CONSTRAINT FK_Strategy_Node_NodeId
              FOREIGN KEY (NodeId)
                             REFERENCES Strategy_Node ) ;


ALTER TABLE Strategy_NodeStrategyRel
       ADD  ( CONSTRAINT FK_Strat_Strat_stratId
              FOREIGN KEY (StrategyId)
                             REFERENCES Strategy_Strategy ) ;


ALTER TABLE Strategy_Strategy
       ADD  ( CONSTRAINT FK_Security_User_authorId
              FOREIGN KEY (AuthorId)
                             REFERENCES Security_User ) ;


ALTER TABLE Strategy_Strategy
       ADD  ( CONSTRAINT FK_SS_Security_User_userId
              FOREIGN KEY (UserId)
                             REFERENCES Security_User ) ;


ALTER TABLE Strategy_StrategyParameter
       ADD  ( CONSTRAINT FK_Strategy_Strategy_ownerId
              FOREIGN KEY (OwnerId)
                             REFERENCES Strategy_Strategy ) ;


ALTER TABLE Strategy_StrategyParameter
       ADD  ( CONSTRAINT FK_Str_ParamType_typeid
              FOREIGN KEY (TypeId)
                             REFERENCES Strategy_ParameterType ) ;


ALTER TABLE StrategyEngine_ExecutionState
       ADD  ( CONSTRAINT FK_Strategy_Node_currnodeId
              FOREIGN KEY (CurrentNodeId)
                             REFERENCES Strategy_Node ) ;


ALTER TABLE StrategyEngine_ExecutionState
       ADD  ( CONSTRAINT FK_App_app_appid
              FOREIGN KEY (ApplicationId)
                             REFERENCES Application_Application ) ;

 ALTER TABLE Application_History
       ADD  ( CONSTRAINT FK_APP_HIST_APP_APP
              FOREIGN KEY (ApplicationId)
                             REFERENCES Application_Application (ApplicationId)
                              ) ;


ALTER TABLE Application_History
       ADD  ( CONSTRAINT FK_APP_HIST_SEC_APP
              FOREIGN KEY (SecurityApplicationId)
                             REFERENCES Security_Application (ApplicationId)
                              ) ;


ALTER TABLE Application_History
       ADD  ( CONSTRAINT FK_APP_HIST_SEC_USER
              FOREIGN KEY (UserId)
                             REFERENCES Security_User (UserId) 
                              ) ;


ALTER TABLE Application_History
       ADD  ( CONSTRAINT FK_APP_HIST_STRAT_NODE
              FOREIGN KEY (CurrentNodeID)
                             REFERENCES Strategy_Node (NodeId) 
                              ) ;

alter table CREDITPRODUCT_STRATEGYREL
  add constraint FK_STRREL_PRODUCTS_ID foreign key (CREDITPRODUCTID)
  references CREDITPRODUCT_PRODUCTS (ID);

alter table CREDITPRODUCT_STRATEGYREL
  add constraint FK_STRREL_STRATEGY_STRATEGYID foreign key (STRATEGYID)
  references STRATEGY_STRATEGY (STRATEGYID);

alter table CREDITPRODUCT_PARAMS
  add constraint FK_PARAM_PRODUCTS_ID foreign key (CREDITPRODUCTID)
  references CREDITPRODUCT_PRODUCTS (ID);


alter table DATASOURCE_STRATEGYREL
  add constraint FK_DSSTR_STRATEGY_STRATEGYID foreign key (STRATEGYID)
  references STRATEGY_STRATEGY (STRATEGYID);

alter table BehavioralReports
  add constraint FK_BRep_STRATEGY_STRATEGYID foreign key (STRATEGYID)
  references STRATEGY_STRATEGY (STRATEGYID);

ALTER TABLE Strategy_Schedule
  ADD CONSTRAINT FK_Schedule_StrategyId FOREIGN KEY (StrategyId) 
  REFERENCES Strategy_Strategy (StrategyId);

alter table STRATEGY_PUBLICREL
  add constraint FK_STRATEGY_PUBLICREL foreign key (PUBLICID)
  references STRATEGY_PUBLICNAME (PUBLICNAMEID);
  
alter table STRATEGY_PUBLICREL
  add constraint FK_STRATEGY_STRATEGYID foreign key (STRATEGYID)
  references STRATEGY_STRATEGY (STRATEGYID);

ALTER TABLE Application_NodeSetting
       ADD ( CONSTRAINT FK_APPNODESETTING_NODEID 
              FOREIGN KEY (NODEID) 
                             REFERENCES STRATEGY_NODE (NODEID));
                             
ALTER TABLE Application_NodeSetting
       ADD ( CONSTRAINT FK_APPNODESETTING_APPID 
              FOREIGN KEY (ApplicationId) 
                             REFERENCES APPLICATION_APPLICATION (ApplicationId));
                        
ALTER TABLE SV_ReportingInfo
       ADD  ( CONSTRAINT FK_SV_RepInfo_Security_User
              FOREIGN KEY (UserId)
                             REFERENCES Security_User (UserId)) ;

alter table Strategy_ParameterType
  add constraint IX_Strategy_ParameterType unique (NAME )
  using index 
  pctfree 10;

alter table STRATEGY_STRATEGY
  add constraint IX_STRATEGY_STRATEGY unique (NAME, ISDELETED)
  using index 
  pctfree 10;
  
  alter table Application_DetailName
  add constraint IX_Application_DetailName unique (NAME)
  using index 
  pctfree 10;
  
  alter table Security_Application
  add constraint IX_Security_Application unique ( NAME , Version )
  using index 
  pctfree 10;
  
  alter table Security_Session
  add constraint IX_Security_Session unique ( AppId,
	SessionId,
	State )
  using index 
  pctfree 10;
  
  alter table Security_User
  add constraint IX_Security_User unique ( UserName ,
	DeleteId )
  using index 
  pctfree 10;
  
  alter table Strategy_Node
  add constraint IX_Strategy_Node unique (GUID, IsDeleted)
  using index 
  pctfree 10;
  
  alter table Strategy_StrategyParameter
  add constraint IX_Strategy_StrategyParameter unique ( Name ,
	OwnerId )
  using index 
  pctfree 10;
  
alter table MENUITEM_STATUS_REL
  add constraint FK_MenuItem_Status foreign key (MENUITEMID)
  references menuitem (ID);
  
alter table MENUITEM_STATUS_REL
  add constraint FK_Status_MenuItem foreign key (STATUSID)
  references appstatus (ID);
  
alter table MenuItem
	add constraint FK_MenuItem_MenuItem foreign key (ParentId)
	references menuitem (ID);