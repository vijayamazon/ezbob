#!/bin/bash

for FILE_NAME in `find /cygdrive/c/ezbob -name "*.csproj"`
do
	DIR_NAME=`dirname ${FILE_NAME}`

	let n=0
	CAI_PATH=

	NOT_FOUND=1

	while [ ${NOT_FOUND} -ne 0 ]
	do
		CAI_PATH=${CAI_PATH}../
		let n=n+1

		\ls ${DIR_NAME}/${CAI_PATH}Common/CommonAssemblyInfo.cs > /dev/null 2> /dev/null

		NOT_FOUND=$?
	done

	\echo ${FILE_NAME} ${n}

	\mv ${FILE_NAME} ${FILE_NAME}.orig-csproj

	awk --assign nLevel=${n} -f /cygdrive/c/temp/add_common.awk ${FILE_NAME}.orig-csproj > ${FILE_NAME}
done

