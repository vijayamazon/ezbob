create or replace function GetAllApplication return sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for
    select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,ss.displayname as strategyname
     ,cp.name as creditproduct_name
     ,Version
     ,case
        when aa.state = 0 then 'NeedProcessBySE'
        when aa.state = 1 then 'NeedProcessByNode'
        when aa.state = 2 then 'StrategyHasBeenFinishedWithoutErrors'
        when aa.state = 3 then 'StrategyHasBeenFinishedWithErrors'
        when aa.state = 4 then 'ArchiveApplication'
        when aa.state = 5 then 'ApplicationError'
        when aa.state = 6 then 'ApplicationWithSecurityViolationError'
        when aa.state = 7 then 'ApplicationWithHandledError'
      end state
     ,aa.errormsg
     ,nvl(childcount.counter, 0) childcount
     ,su.userid as userId
     ,su.username as userName
     ,su.fullname as userFullName
    from
     application_application aa
     left join strategy_strategy ss on ss.strategyid = aa.strategyid
     left join
          (select
              (
                Select
                   rtrim(to_char(sys_xmlagg(xmlelement( "QWER", xmlagg( xmlelement("COL", cpp.name || ',') Order by cpp.name))).extract('ROWSET/QWER/COL/text()').getclobval()),',') TEXT
                from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps1.strategyid = cps.strategyid 
                Group by cpp.name
              ) as name
              , cps1.strategyid 
              from creditproduct_products cpp, creditproduct_strategyrel cps1 where cpp.id = cps1.creditproductid
              group by cps1.strategyid
            ) cp on cp.strategyid = aa.strategyid
     left join
          (select aac.appcounter, count(aac.applicationid) as counter
                  from application_application aac
           where aac.parentappid is not null group by aac.appcounter) childcount on childcount.appcounter = aa.appcounter
     left join security_user su on su.userid = aa.creatoruserid
    where
     aa.parentappid is null;
 return l_cur;
end GetAllApplication;
/
