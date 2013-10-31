using System;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;

namespace com.yodlee.sampleapps
{
	/// <summary>
	// A common super class for all sample applications that helps
	// initialize a client, and establishes a
	// CobrandContext.
	/// </summary>
	public class ApplicationSuper
	{
        public static Hashtable paymentRequestStatusCodes = new Hashtable();
        // stateNames map hardcoded as they are not accesible thru c#
        public static Hashtable stateNames = new Hashtable();
        // countryNames map hardcoded as they are not accesible thru c#
        public static Hashtable countryNames = new Hashtable();
        public static int VERIFICATION_FLAG_VERIFIED_ONLY_ID = 0;
        public static int VERIFICATION_FLAG_UNVERIFIED_ONLY_ID	= 1;
        public static int VERIFICATION_FLAG_EITHER_ID			= 2;

        public static int ACTIVATION_FLAG_ACTIVE_ONLY_ID			= 0;
        public static int ACTIVATION_FLAG_INACTIVE_ONLY_ID			= 1;
        public static int ACTIVATION_FLAG_EITHER_ID					= 2;

        protected CobrandContextSingleton cobCxtSing ;
                
        public ApplicationSuper()
        {
            cobCxtSing = CobrandContextSingleton.Instance;
        }

        public CobrandContext getCobrandContext()
        {
            return cobCxtSing.getCobrandContext();
        }        
    }
}
