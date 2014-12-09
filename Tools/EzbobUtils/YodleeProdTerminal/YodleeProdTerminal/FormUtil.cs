using System;
using System.Text;
//using com.yodlee.common;
//using com.yodlee.soap.core.accountmanagement;
using System.Collections;
using com.yodlee.sampleapps.util;
//using com.yodlee.core.accountmanagement;

namespace com.yodlee.sampleapps.util
{
    /// <summary>
    /// This is a utility class to provide operations that deal with the Form
    /// object inside the Yodlee system.  The Form object represents a web form
    /// that is provided by a site.  Typically these forms are associated with
    /// the Add Item and Autoreg actions for a Content Service.
    /// </summary>
    public class FormUtil
    {
        // The following fields should be enumerated inside the
        // FieldInfoType object, but are not in the C# code.
        private const String IF_LOGIN = "IF_LOGIN";
        private const String IF_TEXT = "TEXT";
        private const String IF_PASSWORD = "IF_PASSWORD";
        private const String IF_OPTIONS = "OPTIONS";
        private const String IF_RADIO = "RADIO";

        private static String FIELDINFO_MULTI_FIXED_MAX_LENGTH = "40";

        /// <summary>
        ///   This method will print to the Console a textual representation of
        ///   the structure of a Form.
        /// </summary>
        /// <param name="form">Provide the form that is traversed</param>
        public static void PrintFormStructureAsHtml(Form form)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<form action=\"#\">\n");
            PrintFormObjectAsHtml(form, stringBuilder, "  ");
            stringBuilder.Append("</form>\n");
            System.Console.WriteLine(stringBuilder.ToString());
            System.Console.WriteLine("");
        }

        /// <summary>
        /// A form has the potential to have many recursive layers.  This method
        /// allows any type of form element to be passed into it, and it determines
        /// which type of element is present and traverses appropriately
        /// </summary>
        /// <param name="formObject">The form object to be processed</param>
        /// <param name="stringBuilder">The StringBuilder that holds all of the output as it is built</param>
        /// <param name="padding">String padding to make the final display pretty</param>
        public static void PrintFormObjectAsHtml(Object formObject, StringBuilder stringBuilder, String padding)
        {
            // If the object is a Form it is either the top-level form, and a subform.
            // Sub-forms are incredibly rare in the Yodlee system.
            if (formObject is Form)
            {
                // A form object itself can be surrounded by a table.  The border is
                // turned on in this instance to make it slightly easier to see the levels
                // in a multi-level form.
                stringBuilder.Append(padding + "<table border=\"1\">\n");
                Form form = (Form)formObject;
                for (int i = 0; i < form.componentList.Length; i++)
                {
                    PrintFormObjectAsHtml(form.componentList[i], stringBuilder, padding + "  ");
                }
                stringBuilder.Append(padding + "</table>\n");
            }
            // FieldInfoSingle is the most common type of object in the Yodlee system.
            // It describes a single Name/Value pair that is presented to the user.
            else if (formObject is FieldInfoSingle)
            {
                FieldInfoSingle fieldInfoSingle = (FieldInfoSingle)formObject;
                stringBuilder.Append(padding + "<tr>\n");
                stringBuilder.Append(padding + "  <td>" + fieldInfoSingle.displayName + ":</td>\n");
                stringBuilder.Append(padding + "  <td>\n");
                if (fieldInfoSingle.fieldType != null)
                {
                    PrintFieldInfoAsHtml(fieldInfoSingle.fieldType.GetType().ToString(),
                        fieldInfoSingle.valueIdentifier,
                        fieldInfoSingle.value,
                        fieldInfoSingle.validValues,
                        fieldInfoSingle.displayValidValues,
                        stringBuilder,
                        padding + "    ");
                }
                else
                {
                    // This is rare condition.  In a completely developed and released
                    // Form, FieldType will never be null.  In certain forms under-
                    // development that may be deployed to a staging environment, it
                    // is possible that this entry could be Null.  The proper behavior
                    // is likely to throw an exception that is caught higher up and
                    // the end user is told the Site is not valid.
                    stringBuilder.Append(padding + "    FieldType is null.\n");
                }
                stringBuilder.Append(padding + "  </td>\n");
                stringBuilder.Append(padding + "</tr>\n");
            }
            // A FieldInfoChoice object contains multiple child FieldInfo objects.
            // The user only need to fill in a single one of these child objects.
            else if (formObject is FieldInfoChoice)
            {
                FieldInfoChoice fieldInfoChoice = (FieldInfoChoice)formObject;
                stringBuilder.Append(padding + "<tr><td colspan=\"2\">\n");
                stringBuilder.Append(padding + "  <table border=\"1\">\n");
                for (int i = 0; i < fieldInfoChoice.fieldInfoList.Length; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(padding + "  <tr><td colspan=\"2\">or</td></tr>\n");
                    }
                    PrintFormObjectAsHtml(fieldInfoChoice.fieldInfoList[i], stringBuilder, padding + "  ");
                }
                stringBuilder.Append(padding + "  </table>\n");
                stringBuilder.Append(padding + "</td></tr>\n");
            }
            // FieldInfoMultiFixed is meant to query for a single value that is broken up
            // up over multiple HTML fields.  Common uses are phone number as three fields,
            // zipcode as two fields or SSN as three fields.
            else if (formObject is FieldInfoMultiFixed)
            {
                FieldInfoMultiFixed fieldInfoMultiFixed = (FieldInfoMultiFixed)formObject;
                stringBuilder.Append(padding + "<tr>\n");
                stringBuilder.Append(padding);
                stringBuilder.Append("  <td>");
                stringBuilder.Append(fieldInfoMultiFixed.displayName);
                stringBuilder.Append("</td>\n");
                stringBuilder.Append(padding + "  <td>\n");
                for (int i = 0; i < fieldInfoMultiFixed.valueIdentifiers.Length; i++)
                {
                    PrintFieldInfoAsHtml(
                        fieldInfoMultiFixed.fieldTypes[i].Value.ToString(),
                        fieldInfoMultiFixed.valueIdentifiers[i],
                        fieldInfoMultiFixed.values[i],
                        fieldInfoMultiFixed.validValues[i],
                        fieldInfoMultiFixed.displayValidValues[i],
                        stringBuilder,
                        padding + "    "
                        );
                }
                stringBuilder.Append(padding + "  </td>\n");
                stringBuilder.Append(padding + "</tr>\n");
            }
            else
            {
                System.Console.WriteLine("Unknowng Form Type: {0}", formObject.GetType());
            }
        }

        /// <summary>
        ///   This method is used to print the HTML for a single form element 
        ///   to the user.  This element may be a text field, password, drop-
        ///   down, etc.
        /// </summary>
        /// <param name="typeName">The name of the type of field it is such as: LOGIN, TEXT, PASSWORD</param>
        /// <param name="valueIdentifier">The unique identifier of the value</param>
        /// <param name="value">The value of the object if it should be populated for the user</param>
        /// <param name="validValues">The range of valid values for a RADIO or OPTIONS</param>
        /// <param name="displayValidValues">The range of valid values to display to the user</param>
        /// <param name="stringBuilder">The StringBuilder to append the HTML into</param>
        /// <param name="padding">How much space to prepend for a pretty output</param>
        public static void PrintFieldInfoAsHtml(
            String typeName,
            String valueIdentifier,
            String value,
            String[] validValues,
            String[] displayValidValues,
            StringBuilder stringBuilder,
            String padding)
        {
            // IF_TEXT and IF_LOGIN can both be treated as plain text boxes.
            // IF_LOGIN generally is reserved for the USERNAME, but they are
            // symantically equivelent.
            if (IF_TEXT.Equals(typeName)
                || IF_LOGIN.Equals(typeName))
            {
                stringBuilder.Append(padding + "<input type=\"text\" name=\"");
                stringBuilder.Append(valueIdentifier);
                stringBuilder.Append("\" value=\"");
                stringBuilder.Append(value);
                stringBuilder.Append("\" size=\"20\" maxlength=\"40\" />\n");
            }
            // IF_PASSWORD should be masked so the user doesn't see what they are
            // typing.  Generally this should not displayed back to the user when
            // fixing an error.
            else if (IF_PASSWORD.Equals(typeName))
            {
                stringBuilder.Append(padding + "<input type=\"password\" name=\"");
                stringBuilder.Append(valueIdentifier);
                stringBuilder.Append("\" value=\"");
                stringBuilder.Append(value);
                stringBuilder.Append("\" size=\"20\" maxlength=\"40\" />\n");
            }
            // Either and options list or a radion button.  Both of these
            // can be displayed as an options list to simply the code.
            else if (IF_OPTIONS.Equals(typeName)
                || IF_RADIO.Equals(typeName))
            {
                stringBuilder.Append(padding + "<select name=\"");
                stringBuilder.Append(valueIdentifier);
                stringBuilder.Append("\">\n");

                for (int i = 0; i < validValues.Length; i++)
                {
                    stringBuilder.Append(padding + "  <option value=\"");
                    stringBuilder.Append(validValues[i]);
                    stringBuilder.Append("\">");
                    stringBuilder.Append(displayValidValues[i]);
                    stringBuilder.Append("</option>\n");
                }
                stringBuilder.Append(padding + "</select>\n");
            }
            else
            {
                // There are other experimental field types such as IF_URL,
                // IF_FILE, but as of 6.2 it is safe to ignore these types.
                // Future iterations of the FORMs may introduce additional
                // types.  The best behavior is to throw an exception
                // and tell the user this is not a valid Content Service.
                System.Console.WriteLine("Unknown FieldType = [" + typeName + "]");
            }

        }

        /// <summary>
        ///   This method will print to the Console a textual representation of
        ///   the structure of a Form.
        /// </summary>
        /// <param name="form">Provide the form that is traversed</param>
        public static void PrintFormStructureAsText(Form form)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PrintFormObjectAsText(form, "  ");
            System.Console.WriteLine(stringBuilder.ToString());
            System.Console.WriteLine("");
        }

        /// <summary>
        /// A form has the potential to have many recursive layers.  This method
        /// allows any type of form element to be passed into it, and it determines
        /// which type of element is present and traverses appropriately
        /// </summary>
        /// <param name="formObject">The form object to be processed</param>
        /// <param name="padding">String padding to make the final display pretty</param>
        public static void PrintFormObjectAsText(Object formObject, String padding)
        {
            if (formObject is Form)
            {
                Form form = (Form)formObject;
                System.Console.WriteLine(padding + "  +-" + form.GetType());
                if (form.conjunctionOp.Value == FormConjunctionOperator.AND)
                {
                    System.Console.WriteLine(padding + "  +-conjunctionOperator = AND");
                }
                else
                {
                    System.Console.WriteLine(padding + "  +-conjunctionOperator = OR");
                }
                System.Console.WriteLine(padding + "  +-components");
                for (int i = 0; i < form.componentList.Length; i++)
                {
                    PrintFormObjectAsText(form.componentList[i], padding + "    ");
                }
            }
            else if (formObject is FieldInfoSingle)
            {
                FieldInfoSingle fieldInfoSingle = (FieldInfoSingle)formObject;
                System.Console.WriteLine(padding + "+-" + fieldInfoSingle.GetType() + ", displayName=" + fieldInfoSingle.displayName);
                System.Console.WriteLine(padding + "  +-FieldType=" + fieldInfoSingle.fieldType.Value.ToString());
            }
            else if (formObject is FieldInfoChoice)
            {
                FieldInfoChoice fieldInfoChoice = (FieldInfoChoice)formObject;
                System.Console.WriteLine(padding + "+-" + fieldInfoChoice.GetType());
                for (int i = 0; i < fieldInfoChoice.fieldInfoList.Length; i++)
                {
                    PrintFormObjectAsText(fieldInfoChoice.fieldInfoList[i], padding + "  ");
                }
            }
            else if (formObject is FieldInfoMultiFixed)
            {
                FieldInfoMultiFixed fieldInfoMultiFixed = (FieldInfoMultiFixed)formObject;
                System.Console.WriteLine(padding + "+-" + fieldInfoMultiFixed.GetType() + ", displayName=" + fieldInfoMultiFixed.displayName);
                for (int i = 0; i < fieldInfoMultiFixed.fieldTypes.Length; i++)
                {
                    System.Console.WriteLine(padding + "  +-FieldType=" + fieldInfoMultiFixed.fieldTypes[i].Value.ToString());
                }
            }
            else
            {
                System.Console.WriteLine(padding + "+-" + formObject.GetType());
            }
        }

        /// <summary>
        /// This method provides a way to use the console to query the user for all
        /// form elements.
        /// </summary>
        /// <param name="formObject">
        ///     When called externally it should be Form, but this method is called recursively
        /// </param>
        /// <param name="fieldInfoList">The list of FieldInfos with the user's response</param>
        public static void getUserInputFieldInfoList(Object formObject, IList fieldInfoList)
        {
            if (formObject is Form)
            {
                Form form = (Form)formObject;
                for (int i = 0; i < form.componentList.Length; i++)
                {
                    getUserInputFieldInfoList(form.componentList[i], fieldInfoList);
                }
            }
            else if (formObject is FieldInfoSingle)
            {
                FieldInfoSingle fieldInfoSingle = (FieldInfoSingle)formObject;

                String result = promptUser(
                        fieldInfoSingle,
                        fieldInfoSingle.validationRules,
                        fieldInfoSingle.value,
                        0,
                        0);

                if (result != null && result.Length > 0)
                {
                    fieldInfoSingle.value = result;
                }

                fieldInfoList.Add(fieldInfoSingle);
            }
            else if (formObject is FieldInfoChoice)
            {
                FieldInfoChoice fieldInfoChoice = (FieldInfoChoice)formObject;
                for (int i = 0; i < fieldInfoChoice.fieldInfoList.Length; i++)
                {
                    getUserInputFieldInfoList(fieldInfoChoice.fieldInfoList[i], fieldInfoList);
                }

            }
            else if (formObject is FieldInfoMultiFixed)
            {
                FieldInfoMultiFixed fieldInfoMultiFixed = (FieldInfoMultiFixed)formObject;
                for (int i = 0; i < fieldInfoMultiFixed.valueIdentifiers.Length; i++)
                {
                    String result = promptUser(
                            fieldInfoMultiFixed,
                            fieldInfoMultiFixed.validationRules[i],
                            fieldInfoMultiFixed.values[i],
                            i,
                            fieldInfoMultiFixed.valueIdentifiers.Length);

                    if (result != null && result.Length > 0)
                    {
                        fieldInfoMultiFixed.values[i] = result;
                    }
                }
                fieldInfoList.Add(fieldInfoMultiFixed);
            }
            else
            {
                // Should throw an error
            }
        }

        public static ArrayList getUserInputFieldInfoList(UserContext userContext, Form inputForm)
        {
            ArrayList fieldInfoList = new ArrayList();

            FormFieldsVisitor visitor = new FormFieldsVisitor(inputForm, userContext);
            while (visitor.hasNext())
            {
                if (visitor.needsBigOr())
                {
                    System.Console.WriteLine("OR");
                }

                bool needsLittleOr = visitor.needsLittleOr();

                FieldInfo fieldInfo = visitor.getNextField();

                if (fieldInfo is FieldInfoSingle)
                {
                    FieldInfoSingle fieldInfoSingle =
                        (FieldInfoSingle)fieldInfo;

                    String valueIdentifier = fieldInfoSingle.valueIdentifier;

                    String[] displayValidValues =
                        fieldInfoSingle.displayValidValues;

                    String[] validValues = fieldInfoSingle.validValues;

                    if (fieldInfoSingle is AutoRegFieldInfoSingle)
                    {
                        //long fieldErrorCode = ((AutoRegFieldInfoSingle)fieldInfoSingle).fieldErrorCode;
                        long fieldErrorCode = (long)((AutoRegFieldInfoSingle)fieldInfoSingle).fieldErrorCode;

                        String fieldErrorMsg = ((AutoRegFieldInfoSingle)fieldInfoSingle).fieldErrorMessage;

                        //if(fieldErrorCode != null){
                        System.Console.WriteLine("(" + fieldErrorMsg + " - " + fieldErrorCode + ")");
                        //}
                    }

                    String value = FormFieldsVisitor.getValue(fieldInfo);
                    // If valueValues is null, then it is a dropdown, else it
                    // is a textbox
                    if (validValues != null)
                    {
                        // Is a drop down
                        System.Console.WriteLine("Drop down values for "
                                + fieldInfo.displayName);

                        for (int i = 0; i < validValues.Length; i++)
                        {
                            System.Console.WriteLine("\tValid value allowed is: "
                                    + validValues[i]);
                        }
                    }

                    // Prompt user to enter value
                    String inValue =
                        promptUser(
                            fieldInfo, fieldInfoSingle.validationRules,
                            value, 0, 0);

                    if (inValue != null)
                    {
                        fieldInfoSingle.value = inValue;
                        fieldInfoList.Add(fieldInfo);
                    }
                }
                else if (fieldInfo is FieldInfoMultiFixed)
                {
                    FieldInfoMultiFixed fieldInfoMultiFixed =
                        (FieldInfoMultiFixed)fieldInfo;

                    String[] ids = fieldInfoMultiFixed.valueIdentifiers;
                    String elementName = null;
                    String[][] validValues =
                        (String[][])fieldInfoMultiFixed.validValues;

                    String[][] displayValidValues =
                        (String[][])fieldInfoMultiFixed.displayValidValues;

                    String[] valueMasks = fieldInfoMultiFixed.valueMasks;
                    String[][] masks = new String[ids.Length][];
                    if (fieldInfoMultiFixed is AutoRegFieldInfoMultiFixed)
                    {

                        long[] fieldErrorCodes = IOUtils.convertNullableLongArrayToLongArray(((AutoRegFieldInfoMultiFixed)fieldInfoMultiFixed).fieldErrorCodes);

                        String[] fieldErrorMsgs = ((AutoRegFieldInfoMultiFixed)fieldInfoMultiFixed).fieldErrorMessages;
                        for (int i = 0; i < fieldErrorCodes.Length; i++)
                        {
                            System.Console.WriteLine("("
                                    + fieldErrorMsgs[i]
                                    + " - "
                                    + fieldErrorCodes[i]
                                    + ")");
                        }
                    }

                    String[] values =
                        FormFieldsVisitor.getValues(fieldInfoMultiFixed);

                    int size = 20;
                    if (ids.Length > 1) size = 7 - ids.Length;
                    String fieldsize = "" + size;
                    String maxlength = FIELDINFO_MULTI_FIXED_MAX_LENGTH;

                    for (int i = 0; i < ids.Length; i++)
                    {
                        elementName = (String)ids[i];

                        // If valueValues is null, then it is a dropdown, 
                        // else it is a textbox
                        if (validValues[i] == null || validValues[i].Length == 0)
                        {
                            // Is a text field
                            String defaultValue = "";
                            if (values != null && values[i] != null)
                            {
                                defaultValue = values[i];
                            }
                        }
                        else
                        {
                            // Is a drop down
                            System.Console.WriteLine("Drop down values for "
                                    + fieldInfo.displayName);

                            for (int j = 0; j < validValues[i].Length; j++)
                            {
                                System.Console.WriteLine("\tVfield #"
                                        + i
                                        + " displayValidValue: "
                                        + displayValidValues[i][j]
                                        + " validValue: "
                                        + validValues[i][j]);
                            }

                        }

                        // Prompt user to enter value
                        // Pass in which index of the multiFixed it is on as well as the max.
                        String inValue =
                            promptUser(fieldInfo,
                                (fieldInfoMultiFixed.validationRules)[i],
                                null, i + 1, ids.Length);

                        values[i] = inValue;
                    }
                    if (values != null)
                    {
                        // check to see that there is no null value in the array
                        bool allNonNull = true;
                        for (int valuesIndex = 0; valuesIndex < values.Length; valuesIndex++)
                        {
                            if (values[valuesIndex] == null)
                            {
                                allNonNull = false;
                                break;
                            }
                        }

                        if (allNonNull)
                        {
                            ((FieldInfoMultiFixed)fieldInfo).values = values;
                            fieldInfoList.Add(fieldInfo);
                        }
                    }

                }
                System.Console.WriteLine("");
            }

            // Convert List to Array
            FieldInfo[] fieldInfos = new FieldInfo[fieldInfoList.Count];
            for (int i = 0; i < fieldInfoList.Count; i++)
            {
                fieldInfos[i] = (FieldInfo)fieldInfoList[i];
            }

            return fieldInfoList;
        }

        public static String promptUser(FieldInfo fieldInfo,
            FieldInputValidationRule[] rules,
            String defaultValue,
            int multiFixedIndex,
            int multiFixedMax)
        {
            bool incomplete = true;
            String inValue = "";

            while (incomplete)
            {
                // Display any validation rules to the user
                if (fieldInfo.helpText != null)
                {
                    System.Console.Write(fieldInfo.helpText + " ");
                }
                else
                {
                    System.Console.Write("No help for field. ");
                }

                if (rules != null)
                {
                    for (int i = 0; i < rules.Length; i++)
                    {
                        System.Console.Write(" ( {0} ) ", rules[i].validationExpression);
                    }
                }
                System.Console.WriteLine("");

                if (multiFixedMax > 0)
                {
                    System.Console.Write("({0} of {1}) ", multiFixedIndex, multiFixedMax);
                }
                // Display default value?
                if (defaultValue != null)
                {
                    System.Console.Write("Please Enter {0} [{1}] : ", fieldInfo.displayName, defaultValue);
                    inValue = System.Console.ReadLine();
                }
                else
                {
                    System.Console.Write("Please Enter {0}: ", fieldInfo.displayName);
                    inValue = System.Console.ReadLine();
                }

                // Check value against validation Expression
                incomplete = false;
            }
            return inValue;

        }

    }
}
