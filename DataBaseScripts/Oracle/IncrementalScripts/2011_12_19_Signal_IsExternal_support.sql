execute immediate 'alter TABLE Signal ADD IsExternal NUMBER NULL';
execute immediate 'create index I_Signal_IsExternal on Signal (ISEXTERNAL)';