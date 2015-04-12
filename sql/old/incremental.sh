#!/bin/bash

DIR="$( cd "$( dirname "${0}" )" && pwd )"

ISQL="${DIR}/osql.exe"

function Say {
	echo "$@"
} # Say

SCRIPTS_PATH=${1}

Say Incremental script started with ...

if [ ! -d "${SCRIPTS_PATH}" ]
then
	Say "Scripts path not found."
	exit 1
fi

HOSTNAME=`hostname | tr [:upper:] [:lower:]`
CONF_FILE_NAME=${DIR}/${HOSTNAME}.conf

Say Reading configuration from ${CONF_FILE_NAME}

while read name val
do
	export ${name}=${val}
done < ${CONF_FILE_NAME}

if [ "x${ODBC}" = "x" ]
then
	Say "Database ODBC source not specified."
	exit 2
fi

if [ "x${DB}" = "x" ]
then
	Say "Database name not specified."
	exit 3
fi

if [ "x${USER}" = "x" ]
then
	Say "Database user not specified."
	exit 4
fi

if [ "x${PASS}" = "x" ]
then
	Say "Database password not specified."
	exit 5
fi

if [ "x${ISQL}" = "x" ]
then
	Say "Query tool path not specified."
	exit 6
fi

Say "Database:   ${DB} via ${ODBC} as ${USER}"
Say "Query tool: ${ISQL}"
Say "Source dir: ${SCRIPTS_PATH}"

function ProcessOneFile {
	local QUERY_FILE=$1

	local OUTPUT_FILE=`dirname "${QUERY_FILE}"`/output.`basename "${QUERY_FILE}"`.txt

	rm -f ${OUTPUT_FILE}

	Say "Running ${QUERY_FILE} ..."

	${ISQL} -h-1 -n -m 11 -e -u -b -D ${ODBC} -d ${DB} -U ${USER} -P ${PASS} -i ${QUERY_FILE} -o ${OUTPUT_FILE}

	local EXIT_CODE=$?

	if [ "${EXIT_CODE}" -eq "0" ]
	then
		Say "Running ${QUERY_FILE} complete."
	else
		Say "Failed with code ${EXIT_CODE} while executing ${QUERY_FILE}."
		Say "Check file ${OUTPUT_FILE} for details."
		PROCESS_ONE_FILE_RESULT=1
		return
	fi

	egrep '^Msg' ${OUTPUT_FILE} > /dev/null

	EXIT_CODE=$?

	if [ "${EXIT_CODE}" -eq "0" ]
	then
		Say "Errors encountered while executing ${QUERY_FILE}."
		Say "Check file ${OUTPUT_FILE} for details."
		PROCESS_ONE_FILE_RESULT=1
		return
	fi

	rm -f ${OUTPUT_FILE}
	PROCESS_ONE_FILE_RESULT=0
} # ProcessOneFile

function ProcessDirectory {
	local THE_DIR=$1
	local QUERY_FILE
	local MARKER_FILE=${THE_DIR}/last.run.marker.file.txt

	for QUERY_FILE in `${DIR}/Bash/ls ${THE_DIR}/*.sql`
	do
		PROCESS_ONE_FILE_RESULT=0

		ProcessOneFile "${QUERY_FILE}"

		if [ "${PROCESS_ONE_FILE_RESULT}" -ne "0" ]
		then
			PROCESS_DIRECTORY_RESULT=1
			return
		fi
	done

	PROCESS_DIRECTORY_RESULT=0
} # ProcessDirectory

PROCESS_DIRECTORY_RESULT=0

ProcessDirectory ${SCRIPTS_PATH}

if [ "${PROCESS_DIRECTORY_RESULT}" -eq "0" ]
then
	Say Incremental script completed successfully, code ${PROCESS_DIRECTORY_RESULT}.
else
	Say Incremental script failed, code ${PROCESS_DIRECTORY_RESULT}.
fi

exit ${PROCESS_DIRECTORY_RESULT}

