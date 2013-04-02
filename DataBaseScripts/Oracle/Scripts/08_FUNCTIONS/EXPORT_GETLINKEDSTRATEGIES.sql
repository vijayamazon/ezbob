CREATE OR REPLACE FUNCTION Export_GetLinkedStrategies
(
  pTemplateId number
)
 return varchar2
AS
  t varchar2(20000);
BEGIN

for rec in (select strategy_strategy.displayname,
  strategy_strategy.termdate
  from export_templatestratrel,
       strategy_strategy
 where strategy_strategy.strategyid = export_templatestratrel.strategyid
 and export_templatestratrel.templateid=pTemplateId)
 loop
 if rec.termdate is null then
    t := t||rec.displayname||';';
 else
    t := t||rec.displayname||' ('||to_char(rec.termdate, 'dd.mm.yyyy HH24:MI:SS') ||');';
  end if;
 end loop;
return t;



END;
/

