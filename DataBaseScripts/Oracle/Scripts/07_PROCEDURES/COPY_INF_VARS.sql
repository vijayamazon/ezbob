create or replace procedure COPY_INF_VARS(pAppId number, pChildAppId number, pAttachCopy number:=1)
-- edited by Kirill Sorudeykin, Oct 31, 2008
 IS

  type t1 is table of number index by binary_integer;
  tmp_detid   t1;
  var_seq     number;
  var_max_old number;
  l_cnt Number;
  l_attach_seq_val Number;
  l_attach_blob Blob;

BEGIN

  select nvl(max(detailid), 0)
    into var_max_old
    from application_detail
    where ApplicationID = pChildAppId;

  for rec1 in (select detailid,
                      DETAILNAMEID,
                      PARENTDETAILID,
                      VALUESTR,
                      VALUENUM,
                      VALUEDATETIME,
                      ISBINARY
                 from application_detail
                 where ApplicationID = pAppId)
   loop


     if  (rec1.isbinary is null or rec1.isbinary!=1 or (pAttachCopy !=0 and rec1.isbinary=1)) then

         -- copy variable detail
        select seq_app_detail.nextval into var_seq from dual;

        tmp_detid(rec1.detailid) := var_seq;

        insert into application_detail
          values
          (var_seq,
           pChildAppId,
           rec1.DETAILNAMEID,
           rec1.PARENTDETAILID,
           rec1.VALUESTR,
           rec1.VALUENUM,
           rec1.VALUEDATETIME,
           rec1.ISBINARY);

       -- determine a number of attachments of current detail
        Select Count(*) into l_cnt
          from Application_Attachment at
          where at.detailId  =  to_number(rec1.detailid);

       -- if attacment exists and we need to copy it
        if (pAttachCopy !=0 and l_cnt > 0) then
        -- copy attachment value
                Select SEQ_APP_ATTACHMENT.Nextval into l_attach_seq_val from dual;

                Select at.value into l_attach_blob from Application_Attachment at
                  where at.detailId  =  rec1.detailid;

                insert into Application_Attachment (ATTACHMENTID,DETAILID, VALUE)
                  values  (l_attach_seq_val, var_seq, l_attach_blob);
        end if;
     end if;
  end loop;

  for rec2 in (select detailid, PARENTDETAILID
                 from application_detail
                where ApplicationID = pChildAppId
                  and detailid > var_max_old)
   loop

    if rec2.parentdetailid is not null then
      update application_detail
         set parentdetailid = tmp_detid(rec2.parentdetailid)
       where detailid = rec2.detailid;
    end if;

  end loop;

  for rec3 in (select name, valuestr, valuenum, valuedatetime
                 from application_detail a, application_detailname b
                where ApplicationID = pChildAppId
                  and (valuestr is not null or valuenum is not null or
                      valuedatetime is not null)
                  and detailid <= var_max_old
                  and a.detailnameid = b.detailnameid)

   loop

    update application_detail
       set valuestr      = rec3.valuestr,
           valuenum      = rec3.valuenum,
           valuedatetime = rec3.valuedatetime
     where applicationid = pChildAppId
       and detailnameid = (select detailnameid
                             from application_detailname
                            where name = rec3.name)
       and detailid > var_max_old;

  end loop;

  delete from application_detail
   where applicationid = pChildAppId
     and detailid <= var_max_old;

-- Added by A.Grechko 23.06.08

update application_detail
   set valuestr = parentdetailid
   where applicationid = pChildAppId
         and detailnameid in
         (select detailnameid
            from application_detailname
            where name = 'ItemDetailId') ;

END COPY_INF_VARS;
/