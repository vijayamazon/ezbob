#!/bin/bash

for FILE_NAME in `find /cygdrive/c/ezbob -name "AssemblyInfo.cs"`
do
	\echo ${FILE_NAME}

	\mv ${FILE_NAME} ${FILE_NAME}.orig-cs

	awk -f /cygdrive/c/temp/filter_ai.awk ${FILE_NAME}.orig-cs > ${FILE_NAME}
done

