CREATE OR REPLACE PROCEDURE CreateHistoricalTables
(pSourceTableName varchar2,
 pHistoryFactsTable varchar2,
 pHistoryFactsSeq varchar2,
 pHistoryTable varchar2,
 pHF2HistLinkTable varchar2)

 AS
BEGIN

  execute immediate 'create table ' || pHistoryTable ||
                    ' (id number, HRDate date, HRType number, ' ||
		    ' HRLoadDate date, HRName VARCHAR2(30),   ' ||
		    ' HRecordAlias number) ';

  execute immediate 'alter table ' || pHistoryTable || ' add constraint PK_' ||
                    pHistoryTable || ' primary key (ID)';

  execute immediate 'alter table ' || pHistoryTable || ' add constraint FK_' ||
                    pHistoryTable || '_HRT foreign key (HRType) references ' ||
                    'HistoryRecordKinds(ID) on delete cascade';

  execute immediate 'create table ' || pHistoryFactsTable ||
                    ' (id number, masterid number) ' ||
		    ' PARTITION BY RANGE (ID) ' ||
		    ' (PARTITION EMPTY VALUES LESS THAN (0))';


  execute immediate 'alter table ' || pHistoryFactsTable || ' add constraint PK_' ||
                    pHistoryFactsTable || ' primary key (ID)';

  execute immediate 'create table ' || pHF2HistLinkTable ||
                    ' (historicalid number, historyrecordid number) ' ||
		    ' PARTITION BY LIST (historyrecordid) ' ||
		    ' (PARTITION EMPTY VALUES (0))';

  execute immediate 'create sequence ' || pHistoryFactsSeq ||
		    ' minvalue 1 maxvalue 999999999999999999999999999' ||
                    ' start with 1 increment by 1 cache 20';

commit;
END;
/
