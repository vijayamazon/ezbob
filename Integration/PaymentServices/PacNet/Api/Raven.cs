using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.API.Support;

namespace Raven.API
{
    public static class Extensions
    {
        /**
         * <p>Answers the URL parameter name used to invoke an operation of
         * the supplied type.</p>
         * 
         * @param ravenOperationType the type of Raven operation
         * @return a string URL parameter name
         */
        public static string GetURLParamName(this RavenOperationType ravenOperationType)
        {

            switch (ravenOperationType)
            {
                case RavenOperationType.HELLO: return "hello";
                case RavenOperationType.SUBMIT: return "submit";
                case RavenOperationType.STATUS: return "status";
                case RavenOperationType.RESPONSE: return "response";
                case RavenOperationType.VOID: return "void";
                case RavenOperationType.CLOSEFILE: return "closefile";
                case RavenOperationType.PAYMENTS: return "payments";
                case RavenOperationType.EVENTS: return "events";
                case RavenOperationType.BROWSERDIRECTPOST: return "directpost";
                default: return "";
            }
        }

        /**
         * <p>Answers a string that can be used to create the base signature for
         * the supplied Raven API object, using a well-defined subset of
         * concatenated properties from the operation.</p>
         *
         * @param ravenOperationType the type of the Raven operation
         * @param aRAPI the Raven API object
         * @return a string of concatenated base operation data sourced from the
         * supplied Raven API object
         * @throws RavenIncompleteSignatureException if the Raven API object is
         * not yet properly configured to be signed
         */
        public static string SignatureRawData(this RavenOperationType ravenOperationType, RavenSecureAPI aRAPI)
        {

            switch (ravenOperationType)
            {
                case RavenOperationType.HELLO:
                    return GetSignatureParam(aRAPI, "UserName");
                case RavenOperationType.SUBMIT:
                    return SignatureBaseRawData(ravenOperationType, aRAPI) +
                        GetSignatureParam(aRAPI, "PymtType", "PaymentType") + GetSignatureParam(aRAPI, "Amount") + GetSignatureParam(aRAPI, "Currency", "CurrencyCode");
                case RavenOperationType.STATUS:
                    return SignatureBaseRawData(ravenOperationType, aRAPI);
                case RavenOperationType.RESPONSE:
                    return SignatureBaseRawData(ravenOperationType, aRAPI);
                case RavenOperationType.VOID:
                    return SignatureBaseRawData(ravenOperationType, aRAPI) +
                        aRAPI.Get("TrackingNumber");
                case RavenOperationType.CLOSEFILE:
                    return SignatureBaseRawData(ravenOperationType, aRAPI) +
                        aRAPI.Get("Filename");
                case RavenOperationType.PAYMENTS:
                    return SignatureBaseRawData(ravenOperationType, aRAPI);
                case RavenOperationType.EVENTS:
                    return SignatureBaseRawData(ravenOperationType, aRAPI);
                case RavenOperationType.BROWSERDIRECTPOST:
                    return SignatureBaseRawData(ravenOperationType, aRAPI) +
                        GetSignatureParam(aRAPI, "PymtType", "PaymentType") + GetSignatureParam(aRAPI, "Amount") + GetSignatureParam(aRAPI, "Currency", "CurrencyCode");
                default:
                    return "";
            }
        }

        /**
         * <p>Answers the string that can be used to create the base signature for
         * the supplied Raven operation, using a well-defined subset of
         * concatenated properties from the operation.</p>
         *
         * @param aRAPI the Raven API object
         * @return a string of concatenated base operation data sourced from the
         * supplied Raven operation
         */
        public static string SignatureBaseRawData(this RavenOperationType ravenOperationType, RavenSecureAPI aRAPI)
        {
            return GetSignatureParam(aRAPI, "UserName") +
                    GetSignatureParam(aRAPI, "Timestamp") +
                    GetSignatureParam(aRAPI, "RequestID");
        }

        /**
         * <p>Answers the value associated with the receiver's signature parameter,
         * if it is set, else throws an exception.</p>
         *
         * @param paramKey the name of the API parameter being retrieved
         * @return the value associated with the parameter key
         * @throws RavenIncompleteSignatureException if the Raven API object is
         * not yet properly configured to be signed
         */
        public static String GetSignatureParam(RavenSecureAPI aRAPI, params String[] paramKeys) {

            String signatureParam = null;

            foreach (String paramKey in paramKeys) 
            {
                signatureParam = aRAPI.Get(paramKey);
                if (signatureParam != null) 
                {
                    break;
                }
            }

            if (signatureParam == null)
            {
                String paramNamesString = "";
                foreach (String paramKey in paramKeys) 
                {
                    paramNamesString += " " + paramKey;
                }
                throw new RavenIncompleteSignatureException(paramNamesString);
            }

            return signatureParam;
        }
    }

    /**
     * An enumeration of valid types of Raven operations.
     */
    public enum RavenOperationType { HELLO, SUBMIT, STATUS, RESPONSE, VOID, CLOSEFILE, PAYMENTS, EVENTS, BROWSERDIRECTPOST };

}
