SCRIPTS_PATH=$1

if [ ! -d "${SCRIPTS_PATH}" ]
then
	echo "Scripts path not found."
	exit
fi

ISQL="C:/Program Files/Microsoft SQL Server/110/Tools/Binn/osql.exe"

while read name val
do
	export ${name}=${val}
done < "`hostname`.conf"

if [ "x${HOST}" = "x" ]
then
	echo "Database host not specifed."
	exit
fi

if [ "x${DB}" = "x" ]
then
	echo "Database name not specifed."
	exit
fi

if [ "x${USER}" = "x" ]
then
	echo "Database user not specifed."
	exit
fi

if [ "x${PASS}" = "x" ]
then
	echo "Database password not specifed."
	exit
fi

if [ "x${ISQL}" = "x" ]
then
	echo "Query tool path not specifed."
	exit
fi

echo "Database:   ${DB} on ${HOST} as ${USER}"
echo "Query tool: ${ISQL}"
echo "Source dir: ${SCRIPTS_PATH}"

OUTPUT_FILE=output.tmp.$$.txt

for QUERY_FILE in `ls ${SCRIPTS_PATH}`
do
	rm -f ${OUTPUT_FILE}

	echo "Running ${QUERY_FILE} ..."

	"${ISQL}" -h-1 -n -m 11 -e -u -b -S ${HOST} -d ${DB} -U ${USER} -P ${PASS} -i ${SCRIPTS_PATH}/${QUERY_FILE} -o ${OUTPUT_FILE}

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

