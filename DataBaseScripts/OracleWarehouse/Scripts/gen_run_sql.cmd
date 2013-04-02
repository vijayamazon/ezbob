@echo off
for /F "usebackq" %%j in (`dir /B /A:D`) do echo %%j && call gen_dir_run.cmd %%j
 