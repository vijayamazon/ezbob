    create table SystemCalendar_Calendar (
        Id NUMBER(10,0) not null,
       IsDeleted NUMBER(10,0),
       IsBase NUMBER(1,0) not null,
       DisplayName NVARCHAR2(255) not null,
       Description NVARCHAR2(255),
       UserId NUMBER(10,0) not null,
       Name NVARCHAR2(255) not null,
       TerminationDate TIMESTAMP(4),
       SignedDocument NCLOB,
       SignedDocumentDelete NCLOB,
       primary key (Id),
      unique (IsDeleted, DisplayName)
    );

    create table SystemCalendar_BaseRelation (
        CalendarId NUMBER(10,0) not null,
       BaseCalendarId NUMBER(10,0) not null
    );

    create table SystemCalendar_Day (
        Id NUMBER(10,0) not null,
       DayOfWeek NVARCHAR2(255) not null,
       IsWorkDay NUMBER(1,0) not null,
       BeginsAt TIMESTAMP(4),
       Duration NUMBER(10,0),
       LunchBeginsAt TIMESTAMP(4),
       LunchDuration NUMBER(10,0),
       HostCalendarId NUMBER(10,0) not null,
       primary key (Id)
    );

    create table SystemCalendar_Entry (
        Id NUMBER(10,0) not null,
       EntryDate TIMESTAMP(4) not null,
       Description NVARCHAR2(255) not null,
       EntryMode NVARCHAR2(255) not null,
       HostCalendarId NUMBER(10,0),
       HostEntryId NUMBER(10,0),
       primary key (Id)
    );

    create table Strategy_CalendarRelation (
        StrategyId NUMBER(10,0) not null,
       CalendarId NUMBER(10,0) not null
    );

    alter table SystemCalendar_BaseRelation 
        add constraint FK9107F620ADD7AFD3 
        foreign key (BaseCalendarId) 
        references SystemCalendar_Calendar;

    alter table SystemCalendar_BaseRelation 
        add constraint FK9107F6204A086D8C 
        foreign key (CalendarId) 
        references SystemCalendar_Calendar;

    alter table SystemCalendar_Day 
        add constraint FKAFEECA076E0ADFD1 
        foreign key (HostCalendarId) 
        references SystemCalendar_Calendar;

    alter table SystemCalendar_Entry 
        add constraint FK5B28FB9C6E0ADFD1 
        foreign key (HostCalendarId) 
        references SystemCalendar_Calendar;

    alter table SystemCalendar_Entry 
        add constraint FK5B28FB9C177ADF22 
        foreign key (HostEntryId) 
        references SystemCalendar_Entry;

    alter table Strategy_CalendarRelation 
        add constraint FKEA3BC91D4A086D8C 
        foreign key (CalendarId) 
        references SystemCalendar_Calendar;

    alter table Strategy_CalendarRelation 
        add constraint FKEA3BC91DC083FE87 
        foreign key (StrategyId) 
        references Strategy_Strategy;

    create sequence SEQ_SystemCalendar_Calendar;

    create sequence SEQ_SystemCalendar_Day;

    create sequence SEQ_SystemCalendar_Entry;