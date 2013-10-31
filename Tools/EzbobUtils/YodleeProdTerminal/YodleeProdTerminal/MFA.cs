using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using com.yodlee.sampleapps.util;
using System.Web.Services.Protocols;

namespace com.yodlee.sampleapps
{

    public class MFA : ApplicationSuper
    {
        private RefreshService refresh;

        public MFA()
        {
            refresh = new RefreshService();
            refresh.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "RefreshService";
        }

        /**
        *  processMFA() 
         *  This method will establish a real time interaction with the agent.   
         *  Agent sends the questions to user and user answers and creates a response and send it to the agent
         *  Agent will stop if the answers are correct or incorrect with the appropriate errorCode. 
         *
        */
        public long processMFA(UserContext userContext, MFARefreshInfo mfaInfo, long? itemId) {
            bool ItemIdSpecified = true;
	        System.Console.WriteLine("Entering MFA flow");
	        //Check MFARefreshInfo is null and then proceed, If there are any questions then MFARefreshInfo will not be null
	        while ( mfaInfo!= null ) {			
	            try {				
		            //First time when the agent has some questions, getErrorCode() will be null
                    if (mfaInfo.errorCode == null && mfaInfo.fieldInfo == null) {
					    return -1;
				    }
                    /**
                     * Use the following code to check errorCode.
                     * use the code mfaInfo.errorCodeSpecified to check the errorCode status
                     */
                    if (mfaInfo.errorCode != null)
                    {
                         long errorCode = mfaInfo.errorCode.Value;
			             //If the getErrorCode() is 0 then it indicates that the agent was able to login to the site with the 
			             //MFA questions successfully
			             if ( errorCode == 0 ) {
				             return errorCode;
			             //If the getErrorCode() is non-zero then it indicates that there was some gatherer error and needs to break from the loop
			             } else if (errorCode > 0) {
				            return errorCode;
			            }
		            }
        			
		            //Check if there are any MFA questions for the user.
		            MFAFieldInfo fieldInfo = mfaInfo.fieldInfo;
        			
		            if ( fieldInfo!= null ) {				
			            long answerTimeout =  mfaInfo.timeOutTime;
			            //If the site is Token based
    		            if ( fieldInfo is TokenIdFieldInfo ) {
    			            System.Console.WriteLine("Inside the token Id");
    			            TokenIdFieldInfo token_fieldInfo = (TokenIdFieldInfo) fieldInfo; 
    			            System.Console.WriteLine(token_fieldInfo.displayString);
                            System.Console.WriteLine("\nYou have " + answerTimeout / 1000 + " seconds to enter the token number");
                            System.Console.WriteLine("\n" + "Enter the token number");     			
    			            //Read the token value 
    			            String tokenId = IOUtils.readStr();
    			            //Create the token response
    			            MFATokenResponse mfatokenresponse = new MFATokenResponse();
    			            mfatokenresponse.token = tokenId;
    			            //Put this MFA Request back in the queue
                            refresh.putMFARequest(userContext, mfatokenresponse, itemId, ItemIdSpecified); 	        			
    		            }//If the site is Image based 
    		            else if ( fieldInfo is ImageFieldInfo ) {
    			            System.Console.WriteLine("Inside Image");
    			            ImageFieldInfo image_fieldInfo = (ImageFieldInfo) fieldInfo;
    			            try {
    				            //Place the image obtained at a particular path for the user to view
    				            String filename = "MFA_" + itemId + ".jpg";
                                FileStream outStream = File.OpenWrite(filename);
                                BufferedStream bufOutStream = new BufferedStream(outStream);
                                //bufOutStream.Write(image_fieldInfo.image, 0, image_fieldInfo.image.Length);
                                bufOutStream.Close();
    	                        System.Console.WriteLine("Image" + filename + " has been placed at XXX" + /*System.getProperty("user.dir")*/  " for viewing.\n");	        	            
    	                    } catch (IOException  e) {
    	        	            System.Console.WriteLine("Exception while writing the image onto the file" + e.Message);
    	                    }
    	                    //Get the corresponding code from the user
    			            System.Console.WriteLine("\n" + "Enter the code present in the image");
    			            String imageCode = IOUtils.readStr();
    			            //Create the MFA response and place it in the queue for the agent to read
    			            MFAImageResponse mfaimageresponse = new MFAImageResponse();
    			            mfaimageresponse.imageString = imageCode;
                            refresh.putMFARequest(userContext, mfaimageresponse, itemId, ItemIdSpecified);
    		            } //If the site is Security Question type 
    		            else if ( fieldInfo is SecurityQuestionFieldInfo ) {
    			            SecurityQuestionFieldInfo securityqa_fieldInfo = (SecurityQuestionFieldInfo) fieldInfo;        			
    			            QuestionAndAnswerValues[] queAndAns = securityqa_fieldInfo.questionAndAnswerValues;
    			            // Create the MFA response for security questions
    			            MFAQuesAnsResponse mfaqaResponse = new MFAQuesAnsResponse();
    			            QuesAndAnswerDetails[] qaDetails = new QuesAndAnswerDetails[queAndAns.Length]; 
    			            int count =0;
    			            System.Console.Write("\nYou have " + answerTimeout/1000 + " seconds to answer the questions");
    			            for ( int loopcounter=0; loopcounter < queAndAns.Length ; loopcounter++) {
    				            if ( queAndAns[loopcounter] is SingleQuesSingleAnswerValues ) {
    					            //Get the question	        					        					
    					            String mfa_ques = ((SingleQuesSingleAnswerValues)queAndAns[loopcounter]).question;
    					            System.Console.Write("\n" + mfa_ques);
    					            //Get the answer
    					            System.Console.Write("\nAnswer: ");
    					            String mfa_answer = IOUtils.readStr().Trim();
    					            //Get the MFA_TYPE	        					
    					            String que_type = ((SingleQuesSingleAnswerValues)queAndAns[loopcounter]).questionFieldType;
    					            //Get the answer field type 
    					            String ans_type = ((SingleQuesSingleAnswerValues)queAndAns[loopcounter]).responseFieldType;
    					            //Get the metadata 
    					            String metadata = ((SingleQuesSingleAnswerValues)queAndAns[loopcounter]).metaData;
    					            //Create the Response using the question & answer        					
    					            QuesAndAnswerDetails mfaqa_details = new QuesAndAnswerDetails();
                                    mfaqa_details.question = mfa_ques;
                                    mfaqa_details.answer = mfa_answer;
                                    mfaqa_details.questionFieldType = que_type;
                                    mfaqa_details.answerFieldType = ans_type;
                                    mfaqa_details.metaData = metadata;
        				            //mfa_qa.add(mfaqa_details);
    					            qaDetails[count++] = mfaqa_details ;	            				
    				            } else if (queAndAns[loopcounter] is MultiQuesMultiAnswerOptionsValues ) {
    					            //This is for sites having checkbox or radio buttons
    					            //ToDo:Will be implemented later 
    					            System.Console.WriteLine("Inside MultiQuesMultiAnswerOptionsValues");
    				            } else if ( queAndAns[loopcounter] is MultiQuesOptionsSingleAnswerValues) {
    					            //This is for sites having checkbox or radio buttons
    					            //ToDo:Will be implemented later
    					            System.Console.WriteLine("Inside MultiQuesOptionsSingleAnswerValues");
    				            } else if ( queAndAns[loopcounter] is SingleQuesMultiAnswerOptionsValues) {
    					            //This is for sites having checkbox or radio buttons
    					            //ToDo:Will be implemented later
    					            System.Console.WriteLine("Inside SingleQuesMultiAnswerOptionsValues");        					
    				            }       				
    			            }
    			            //Create the response and place it in the queue for the agent to read
    			            mfaqaResponse.quesAnsDetailArray = qaDetails;
                            refresh.putMFARequest(userContext, mfaqaResponse, itemId, ItemIdSpecified);	        			
    		            }
    	              }
    	            //Get the MFA response from the agent, which contains the MFA questions
		            //The questions will be placed in the MQ and the app or SDK calls can poll for these questions continuously
                    mfaInfo = refresh.getMFAResponse(userContext, itemId, ItemIdSpecified);        	
                } catch (SoapException e)
                {
                    System.Console.WriteLine("Exception in processMFA method in MFA class:" + e.Message);
                    System.Console.WriteLine(e.StackTrace);
                }
            } //End of while
	            return -1;    		
        }
    }
}