IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededNodes]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExceededNodes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExceededNodes] 
	(@iAppId bigint)
AS
BEGIN
	select
   *
   from
   (
        select 
           nodes.id
          ,nodes.applicationid
          ,nodes.nodeid
          ,nodes.name
          ,nodes.displayname
          ,nodes.workplace_name
          ,nodes.user_name
          ,nodes.value_of_limit
          ,nodes.commingtime
          ,nodes.exittime
          ,nodes.odds_exittime_commingtime
          ,nodes.value_of_exceeded
        from
        (
          select
           an.id
          ,an.applicationid
          ,sn.nodeid
          ,sn.name
          ,sn.displayname
          ,(select sa.name from security_application sa where sa.applicationid = sn.applicationid) as workplace_name
          ,(select su.fullname from security_user su where su.userid = an.userid) as user_name
          ,sn.executionduration as value_of_limit
          ,(select anc.comingtime from application_nodetime anc where anc.id = an.id) as commingtime
          ,(select ISNULL(ane.exittime, GETDATE()) from application_nodetime ane where ane.id = an.id) as exittime
          ,round(
					(
						DATEDIFF(s, 
								 (select anc.comingtime from application_nodetime anc where anc.id = an.id), 
								 (select ISNULL(ane.exittime, GETDATE()) from application_nodetime ane where ane.id = an.id)
							    )
					), 0) as odds_exittime_commingtime
          ,
          case
            when sn.executionduration > 
					round(
							(
								DATEDIFF(s, 
										 (select anc.comingtime from application_nodetime anc where anc.id = an.id), 
										 (select ISNULL(ane.exittime, GETDATE()) from application_nodetime ane where ane.id = an.id)
										)
							), 0)
            then 0
            else
				 round(
						(
							DATEDIFF(s, 
									 (select anc.comingtime from application_nodetime anc where anc.id = an.id), 
									 (select ISNULL(ane.exittime, GETDATE()) from application_nodetime ane where ane.id = an.id)
									)
						), 0) - sn.executionduration
          end as value_of_exceeded

          from
           application_nodetime an
           ,strategy_node sn
          where
                sn.nodeid = an.nodeid
           and (sn.executionduration is not null or sn.executionduration != 0)
        ) nodes
        ,application_application aa
        where
          nodes.applicationid = aa.applicationid
          and aa.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = @iAppId)
      ) t
   where
     t.value_of_exceeded > 0
   order by t.id
END
GO
