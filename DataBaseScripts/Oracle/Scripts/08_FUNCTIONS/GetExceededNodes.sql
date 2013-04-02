create or replace function GetExceededNodes(iAppId in number)
return sys_refcursor
as
 l_cur sys_refcursor;
begin
 open l_cur for
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
          ,(select nvl(ane.exittime, sysdate) from application_nodetime ane where ane.id = an.id) as exittime
          ,round(
            ((select nvl(ane.exittime, sysdate) from application_nodetime ane where ane.id = an.id) -
            (select anc.comingtime from application_nodetime anc where anc.id = an.id)) * 24 * 3600
          ) as odds_exittime_commingtime
          ,
          case
            when sn.executionduration > round(((select nvl(ane.exittime, sysdate) from application_nodetime ane where ane.id = an.id) - (select anc.comingtime from application_nodetime anc where anc.id = an.id)) * 24 * 3600)
            then 0
            else
            round(
            ((select nvl(ane.exittime, sysdate) from application_nodetime ane where ane.id = an.id) -
            (select anc.comingtime from application_nodetime anc where anc.id = an.id)) * 24 * 3600
          ) -  sn.executionduration
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
          and aa.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = iAppId)
      ) t
   where
     t.value_of_exceeded > 0
   order by t.id;

return l_cur;
end GetExceededNodes;
/
