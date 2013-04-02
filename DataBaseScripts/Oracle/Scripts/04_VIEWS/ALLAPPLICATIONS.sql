CREATE OR REPLACE VIEW ALLAPPLICATIONS AS
SELECT     aa.applicationid AS AppId, aa.appcounter AS AppCounter, aa.creationdate AS CreationDate, ss.displayname AS StrategyName, ss.StrategyId AS StrategyId,
                      cp.name AS CreditProductName, Version, NULL AS GCRecord, aa.applicationid AS OID, 0 AS OptimisticLockField, aa.state, aa.errormsg AS ErrorMessage,
                      NVL(childcount.counter, 0) ChildCount, su.userid AS UserId, su.username AS UserName, su.fullname AS UserFullName
FROM         application_application aa LEFT JOIN
                      strategy_strategy ss ON ss.strategyid = aa.strategyid
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

        LEFT JOIN      (SELECT     aac.appcounter, count(aac.applicationid) AS counter
                            FROM          application_application aac
                            WHERE      aac.parentappid IS NOT NULL
                            GROUP BY aac.appcounter) childcount ON childcount.appcounter = aa.appcounter LEFT JOIN
                      security_user su ON su.userid = aa.creatoruserid
WHERE     aa.parentappid IS NULL
/
