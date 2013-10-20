#!/bin/bash

echo Incremental script started...

SCRIPTS_PATH=${1}

if [ ! -d "${SCRIPTS_PATH}" ]
then
	echo "Scripts path not found."
	exit
fi

DIR="$( cd "$( dirname "${0}" )" && pwd )"

ISQL="${DIR}/osql.exe"

HOSTNAME=`hostname | tr [:upper:] [:lower:]`
CONF_FILE_NAME=${DIR}/${HOSTNAME}.conf

echo Reading configuration from ${CONF_FILE_NAME}

while read name val
do
	export ${name}=${val}
done < ${CONF_FILE_NAME}

if [ "x${ODBC}" = "x" ]
then
	echo "Database ODBC source not specified."
	exit
fi

if [ "x${DB}" = "x" ]
then
	echo "Database name not specified."
	exit
fi

if [ "x${USER}" = "x" ]
then
	echo "Database user not specified."
	exit
fi

if [ "x${PASS}" = "x" ]
then
	echo "Database password not specified."
	exit
fi

if [ "x${ISQL}" = "x" ]
then
	echo "Query tool path not specified."
	exit
fi

echo "Database:   ${DB} via ${ODBC} as ${USER}"
echo "Query tool: ${ISQL}"
echo "Source dir: ${SCRIPTS_PATH}"

OUTPUT_FILE=${DIR}/output.tmp.$$.txt

for QUERY_FILE in `ls ${SCRIPTS_PATH}/*.sql`
do
	rm -f ${OUTPUT_FILE}

	echo "Running ${QUERY_FILE} ..."

	${ISQL} -h-1 -n -m 11 -e -u -b -D ${ODBC} -d ${DB} -U ${USER} -P ${PASS} -i ${QUERY_FILE} -o ${OUTPUT_FILE}

	EXIT_CODE=$?

	if [ "${EXIT_CODE}" -eq "0" ]
	then
		echo "Running ${QUERY_FILE} complete."
	else
		echo "Failed with code ${EXIT_CODE} while executing ${QUERY_FILE}."
		echo "Check file ${OUTPUT_FILE} for details."
		break
	fi

	egrep '^Msg' ${OUTPUT_FILE} > /dev/null

	EXIT_CODE=$?

	if [ "${EXIT_CODE}" -eq "0" ]
	then
		echo "Errors encountered while executing ${QUERY_FILE}."
		echo "Check file ${OUTPUT_FILE} for details."
		break
	fi

	rm -f ${OUTPUT_FILE}
done

echo Incremental script complete.


