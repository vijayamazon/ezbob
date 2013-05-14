using System.Collections;

namespace YodleeLib
{
    /// <summary>
    // A common super class for all sample applications that helps
    // initialize a client, and establishes a
    // CobrandContext.
    /// </summary>
    public class ApplicationSuper
    {
        public static Hashtable PaymentRequestStatusCodes = new Hashtable();
        // stateNames map hardcoded as they are not accesible thru c#
        public static Hashtable StateNames = new Hashtable();
        // countryNames map hardcoded as they are not accesible thru c#
        public static Hashtable CountryNames = new Hashtable();
        public static int VerificationFlagVerifiedOnlyId = 0;
        public static int VerificationFlagUnverifiedOnlyId = 1;
        public static int VerificationFlagEitherId = 2;

        public static int ActivationFlagActiveOnlyId = 0;
        public static int ActivationFlagInactiveOnlyId = 1;
        public static int ActivationFlagEitherId = 2;

        protected CobrandContextSingleton CobCxtSing;

        public ApplicationSuper()
        {
            CobCxtSing = CobrandContextSingleton.Instance;
        }

        public CobrandContext GetCobrandContext()
        {
            return CobCxtSing.getCobrandContext();
        }
    }
}
