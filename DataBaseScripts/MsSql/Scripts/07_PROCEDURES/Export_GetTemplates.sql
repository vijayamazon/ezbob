IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetTemplates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetTemplates]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplates]

AS
BEGIN
   select
     lst.id,
     lst.filename,
     lst.description,
     lst.uploaddate,
     lst.exceptiontype,
     lst.terminationdate,
     lst.displayname,
     (select * from Export_GetEntityLinkSeries(lst.id)) as "LinkedTo",
     (select count(nodeid) from export_templatenoderel where templateid=lst.id) as "nodescount",
     (select count(strategyid) from export_templatestratrel where templateid=lst.id) as "stratcount",
     (select 1 as Tag, NULL as Parent, b.name as [InfoString!1!]  from  export_templatenoderel a, strategy_node b
	  where a.templateid = lst.id and a.nodeid = b.nodeid for xml explicit) as "nodeslist",
     (select 1 as Tag, NULL as Parent, 
       CASE WHEN (strategy_strategy.TermDate IS NOT NULL) THEN 
         strategy_strategy.DisplayName + ' ('+CONVERT(NVARCHAR, strategy_strategy.TermDate, 120) + ')' 
       ELSE strategy_strategy.DisplayName  
       END as [InfoString!1!]
	  from export_templatestratrel,
		   strategy_strategy
	  where strategy_strategy.strategyid = export_templatestratrel.strategyid
	    and export_templatestratrel.templateid=lst.id for xml explicit) as "stratlist",
     (select fullname from security_user where userid = lst.userid) as "username",
     (
      select
        1 as Tag,
        NULL as Parent,
        t.id as [InfoString!1!]
      from
        entitylink l inner join Export_TemplatesList t on 
            l.EntityId = t.id 
        and l.EntityType='ExportTemplate'
        and t.IsDeleted is null
      where
        l.seriaId=lst.id for xml explicit) as "linkedfromlist"
   from export_templateslist lst 
   where lst.isdeleted is null ;
END;
GO
