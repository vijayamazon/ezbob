CREATE TABLE Filter (
       Id 				NUMBER NOT NULL,
       FilterType 		VARCHAR2(256),
       Status 			VARCHAR2(256),
       Statuses 		VARCHAR2(2048),
       States 			VARCHAR2(2048),
       Name 			VARCHAR2(256)
);

insert into Filter (Id, FilterType, Name, Status) values ('1', 'KISimpleStatusFilter', 'Init', 'Init');
insert into Filter (Id,FilterType, Name, Status) values ('2', 'KISimpleStatusFilter', 'KI_Input', 'KI_Input');
insert into Filter (Id,FilterType, Name, Status) values ('3', 'KISimpleStatusFilter', 'KI_Rework', 'KI_Rework');
insert into Filter (Id,FilterType, Name, Status) values ('4', 'SimpleStatusFilter', 'SM_Review', 'SM_Review');
insert into Filter (Id,FilterType, Name, Status) values ('5', 'SimpleStatusFilter', 'RM_Review', 'RM_Review');
insert into Filter (Id,FilterType, Name, Statuses, States) values ('6', 'KIMultiStatusFilter', 'Rejected', 'Rejected;ClientRefuse', '1');
insert into Filter (Id,FilterType, Name, Status) values ('7', 'SimpleStatusFilter', 'HD_Review', 'HD_Review');
insert into Filter (Id,FilterType, Name, Status) values ('8', 'KISimpleStatusFilter', 'Approved', 'Approved');
insert into Filter (Id,FilterType, Name, Statuses, States) values ('9', 'MultiStatusFilter', 'Archive', 'Archive_CreditGiven;Archive_ClientRefuse;Archive_BankReject', '2');
insert into Filter (Id,FilterType) values ('10', 'StatesFilter');
insert into Filter (Id,FilterType) values ('11', 'KIStatesFilter');
