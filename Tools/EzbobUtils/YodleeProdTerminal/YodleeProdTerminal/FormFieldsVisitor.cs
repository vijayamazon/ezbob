using System;
using System.Collections.Generic;
using System.Text;

//using com.yodlee.core;
//using com.yodlee.common;
//using com.yodlee.core.accountmanagement;
using System.Collections;

/**
 * Used to traverse the entire form and provide a flattened
 * view of the form for iteration.
 * 
 * Includes basic support for OR conjuctions in the form.
 */
namespace com.yodlee.sampleapps
{

    public class FormFieldsVisitor
    {
        /** Holds the list of form fields. */
        private LinkedList<Object> fieldInfoQueue = new LinkedList<Object>();

        /** 
         * Set of elements that are the start of the second group in a big or. 
         */
        private ArrayList needsBigOrSet = new ArrayList();

        /** Set of elements that are in a little-or group. */
        private ArrayList needsLittleOrSet = new ArrayList();

        /** Holds the cobrandContext. Not used. */
        private CobrandContext cobrandContext;
        
        /**
         * Basic constructor will generate the form vistor.
         * 
         * @param form the form to wrap with a vistor
         * @param cobrandContext the context this form came from
         */
        public FormFieldsVisitor(Form form, CobrandContext cobrandContext)
        {
            this.cobrandContext = cobrandContext;
            populateQueue(form);
        }

        /**
         * Populated the queue for the given form.  This can be
         * called recursively for forms that contain subforms with
         * OR conjunctions.
         * 
         * @param form the form to cycle through and populate the queue
         */
        private void populateQueue(Form form) {
            Object[] obj = form.componentList;
            FormComponent[] children = new FormComponent[0];
            if (obj != null) {
                children = new FormComponent[obj.Length];
                for (int i = 0; i < obj.Length; i++)
                {
                    children[i] = (FormComponent)obj[i];
                }
            }             
            //FormComponent[] children = (FormComponent[])form.componentList;
            
            // Find the FieldInfo where we need to put the big OR right in front.
            // These are added intot he needsBigOr set.
            if (FormConjunctionOperator.OR.Equals(form.conjunctionOp)) {
                for (int i = 1; i < children.Length; i++) {
                    if (children[i] is FieldInfo) {
                        needsBigOrSet.Add(children[i]);
                    } else if (children[i] is Form) {
                        FieldInfo found = findLeftMostFieldInfo((Form) children[i]);
                        if (found != null) {
                            needsBigOrSet.Add(found);
                        }
                    }
                }
            }
            
            // Iterate through all the fields recursing as needed for
            // Form and FieldInfoChoice
            for (int i = 0; i < children.Length; i++) {
                if (children[i] is FieldInfo) {
            	    // For FieldInfo, add them straight into the queue
            	    // but mark them as little-ors if needed.
            	    populateQueue((FieldInfo)children[i]);
                	
                    if (FormConjunctionOperator.OR.Equals(form.conjunctionOp) && i != 0) {
                        needsLittleOrSet.Add(children[i]);
                    }            	
                } else if (children[i] is FieldInfoChoice) {
            	    // For FieldInfoChoice, add all of the child components to
            	    // the queue, and add a little or before each child component
            	    // after the first.
            	    FieldInfo[] fieldInfoArray = 
            		    (FieldInfo[])((FieldInfoChoice)children[i]).fieldInfoList;
                	
            	    for(int j = 0; j < fieldInfoArray.Length; j++) {
            		    populateQueue(fieldInfoArray[j]);
            		    if(j > 0) {
            			    needsLittleOrSet.Add(fieldInfoArray[j]);
            		    }
            	    }
                	
                } else if (children[i] is Form) {
            	    // For a form, recurse
                    populateQueue((Form) children[i]);
                }
            }
        }

        /**
         * Add the field info to the queue.  If it is a password type,
         * the additionally add a second field for a password verify
         * to the queue
         * 
         * @param fieldInfo the info to add to the queue
         */
        private void populateQueue(FieldInfo fieldInfo) {
            fieldInfoQueue.AddLast(fieldInfo);
            
            if (fieldInfo is AutoRegFieldInfoSingle) {
                AutoRegFieldInfoSingle field = (AutoRegFieldInfoSingle) fieldInfo;
                FieldType fieldType = (FieldType)field.fieldType;
                String value = getValue(field);
                
                // If the field is a password field, then add a verify field
                // into the system.                 
                if (fieldType.ToString().Equals(FieldType.PASSWORD))
                {
                    AutoRegFieldInfoSingle verify = new AutoRegFieldInfoSingle();
                    verify.value = null;
                    verify.displayName = "Verify " + field.displayName;
                    verify.isEditable = true;
                    verify.isOptional = field.isOptional;
                    verify.validValues = field.validValues;
                    verify.displayValidValues = field.displayValidValues;
                    verify.valueIdentifier = field.valueIdentifier;
                    verify.valueMask = field.valueMask;
                    verify.fieldType = field.fieldType;
                    verify.helpText = "";
                    verify.autoGeneratable = field.autoGeneratable;
                    verify.okToAutoGenerate = field.okToAutoGenerate;
                    verify.fieldErrorCode = field.fieldErrorCode;
                    verify.fieldErrorMessage = field.fieldErrorMessage;
                    verify.validationRules = field.validationRules;
                    verify.size = field.size;
                    verify.maxlength = field.maxlength;
					verify.userProfileMappingExpression = "";                    
                    fieldInfoQueue.AddLast(verify);
                }
            } else if (fieldInfo is FieldInfoSingle) {
                FieldInfoSingle field = (FieldInfoSingle) fieldInfo;
                FieldType fieldType = (FieldType)field.fieldType;
                String value = getValue(field); 

                // If the field is a password field, then add a verify field
                // into the system.
                if (fieldType.ToString().Equals(FieldType.PASSWORD.ToString()))
                {                    
                    FieldInfoSingle verify = new FieldInfoSingle();
                    verify.value = "";
                    verify.displayName = "Verify " + field.displayName;
                    verify.isEditable = true;
                    verify.isOptional = field.isOptional;
                    verify.validValues = field.validValues;
                    verify.displayValidValues = field.displayValidValues;
                    verify.valueIdentifier = field.valueIdentifier;
                    verify.valueMask = field.valueMask;
                    verify.fieldType = field.fieldType;
                    verify.helpText = "";
                    verify.validationRules = field.validationRules;
                    verify.size = field.size;
                    verify.maxlength = field.maxlength;
					verify.userProfileMappingExpression = "";                    
                    fieldInfoQueue.AddLast(verify);
                }
            }
        }

        /**
         * Determines the first FieldInfo object that inside the OR that is
         * returned by the visitor.
         * 
         * @param form the form to search for the leftmost component of
         * @return the left most component
         */
        private FieldInfo findLeftMostFieldInfo(Form form) {
    	    FormComponent[] children = (FormComponent[])form.componentList;
    	    FieldInfo leftMostFieldInfo = null;
        	
    	    if(children.Length > 0) {
    		    FormComponent leftMostFormComponent = children[0];
            
                if (leftMostFormComponent is FieldInfo) {
            	    // If the left most component is a FieldInfo, then its done. 
            	    leftMostFieldInfo = (FieldInfo) leftMostFormComponent;
                    
                } else if(leftMostFormComponent is FieldInfoChoice) {
                    // If the left most component is a FieldInfoChoice, then
                    // retrieve it's left-most component
            	    FieldInfo[] fieldInfoArray = (FieldInfo[])((FieldInfoChoice)leftMostFormComponent).fieldInfoList;
            	    if(fieldInfoArray.Length > 0) {
            		    leftMostFieldInfo = fieldInfoArray[0];
            	    }
                	
                } else if (leftMostFormComponent is Form) {
                    // If the left most component is a subform, recurse
            	    leftMostFieldInfo = findLeftMostFieldInfo((Form) leftMostFormComponent);
                } else {
            	    throw new Exception("Unknown how to process "
            			    + "FormComponent of type [" 
						    + leftMostFormComponent
						    + "]" );
                }
    	    }            
            return leftMostFieldInfo;
        }

        /**
         * Indicates if there are additional fields in the queue
         * 
         * @return true if there are more fields
         */
        public bool hasNext()
        {
            return fieldInfoQueue.Count != 0;
        }

        /**
         * Indicates if the current field is the start of a second
         * part of a big-or conjunction.
         * 
         * @return if it is a big or
         */
        public bool needsBigOr()
        {
            return needsBigOrSet.Contains(fieldInfoQueue.First);
        }

        /**
         * Indicates if the current field is the start of a later part
         * of a little-or conjunction
         * 
         * @return if it is a little or
         */
        public bool needsLittleOr()
        {
            return needsLittleOrSet.Contains(fieldInfoQueue.First);
        }

        /**
         * Returns the next field in the visitor.
         * 
         * @return next field
         */
        public FieldInfo getNextField()
        {
            LinkedListNode<Object> node = fieldInfoQueue.First;
            FieldInfo fieldInfo = (FieldInfo)node.Value;
            fieldInfoQueue.RemoveFirst();
            return fieldInfo;
        }
        
        public static String getValue (FieldInfo fieldInfo)
        {
            if (fieldInfo == null) {
                throw new Exception ("Null FieldInfo argument not legal");
            }
            if (fieldInfo is FieldInfoSingle) {
                FieldInfoSingle fis = (FieldInfoSingle) fieldInfo;
                return escape (fis.value);
            }
            else {
                throw new Exception("Invalid invocation - FieldInfo is multi-valued");
            }
        }

        /**
         * Returns the values of this field. (Valid only for a multi-valued
         * field).
         * <p>
         * @return The field values.
         */
        public static String[] getValues(FieldInfo fieldInfo)
        {
            String[] escapedValues = null;
            if (fieldInfo != null) {
                if (fieldInfo is FieldInfoMultiFixed) {
                    FieldInfoMultiFixed fimf = (FieldInfoMultiFixed) fieldInfo;
                    if (fimf.values != null) {
                        int valuesSize = fimf.values.Length;
                        escapedValues = new String[valuesSize];
                        for (int i=0; i < valuesSize; i++) {
                            escapedValues[i] = escape (fimf.values[i]);
                        }
                    }                    
                } else if (fieldInfo is FieldInfoMultiVariable) {
                    // TODO: escape!
                    FieldInfoMultiVariable fimv = (FieldInfoMultiVariable) fieldInfo;
                    if (fimv.values != null) {
                        int valuesSize = fimv.values.Length;
                        escapedValues = new String[valuesSize];
                        for (int i=0; i < valuesSize; i++) {
                            escapedValues[i] = escape (fimv.values[i]);
                        }
                    }
                }                  
            }
            return escapedValues;
        }

        private static String escape(String orig)
        {
            String ret = null;
            if (orig != null)
            {
                orig = orig.Replace("&", "&amp;");
                orig = orig.Replace("&amp;amp;", "&amp;");
                orig = orig.Replace("\"", "&quot;");
                orig = orig.Replace("&amp;quot;", "&quot");
                orig = orig.Replace("'", "&apos;");
                orig = orig.Replace("&amp;apos;", "&apos;");
                orig = orig.Replace("<", "&lt;");
                orig = orig.Replace("&amp;lt;", "&lt;");
                orig = orig.Replace(">", "&gt;");
                orig = orig.Replace("&amp;gt", "&gt;");
                ret = orig;
            }
            return ret;
        }
    }
}