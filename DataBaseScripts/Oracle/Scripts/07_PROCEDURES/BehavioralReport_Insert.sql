create or replace procedure BehavioralReport_Insert
(
pStrategyId in number,
pReportName in varchar2,
pReportTypeId in number,
pPath in varchar2,
pIsTestRun in number,
pReportId out number
)
as
  l_ReportId Number;
begin

select SEQ_BehavioralReport.nextval into l_ReportId from dual;

insert into BehavioralReports
(ID, STRATEGYID, NAME, TYPEID, PATH, CREATIONDATE, TESTRUN, ISNOTREAD)
values
(
l_ReportId,
pStrategyID,
pReportName,
pReportTypeId,
pPath,
sysdate,
pIsTestRun,
1
);

pReportId := l_ReportId;

end BehavioralReport_Insert;
/
