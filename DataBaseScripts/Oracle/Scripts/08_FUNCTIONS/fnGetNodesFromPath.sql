CREATE OR REPLACE FUNCTION fnGetNodesFromPath
-- Created by A.Grechko
-- Date 10.12.07
(pPath  varchar2  )

RETURN tab_fnGetNodesFromPath pipelined

AS
    pnameNode varchar2(255);
    pindexStart number;
    pindexFinish number;
    p_cnt number;
    lPath varchar2 (10000) ;

BEGIN

 lPath := pPath ; 
 pnameNode := '';
 pindexStart := 1;
 pindexFinish := 0;
 p_cnt := 1;

  -- delete last '/'
 pindexFinish := instr( lPath,'/', LENGTH(lPath) - 1);

  WHILE (pindexFinish <> 0) LOOP

   lPath := substr(lPath, 1, LENGTH(lPath) - 1) ;
   pindexFinish := instr(lPath, '/', LENGTH(lPath) - 1);
  END LOOP ;

 pindexFinish := instr( lPath, '/');

  -- delete first '/'
  WHILE(pindexFinish = 1) LOOP

   lPath := substr(lPath, pindexStart + 1, LENGTH(lPath) - 1)  ;
   pindexFinish := instr( lPath, '/',  pindexStart);
  END LOOP;

  -- gets nodes
  WHILE (pindexStart <> 0) LOOP

    IF (pindexFinish = 0) THEN
     pnameNode := substr(lPath, pindexStart, LENGTH(lPath) - (pindexStart - 1));
    ELSE
     pnameNode := substr(lPath, pindexStart, pindexFinish - pindexStart);

    end if;

    IF (pindexFinish = 0) THEN
     pindexStart := 0;
    ELSE

     pindexStart := pindexFinish + 1;
     pindexFinish := instr( lPath, '/',  pindexStart);

    END IF;

   pipe row (rec_fnGetNodesFromPath (p_cnt, pnameNode)) ;

    p_cnt := p_cnt + 1 ;

  END LOOP;
return;
  END;
/