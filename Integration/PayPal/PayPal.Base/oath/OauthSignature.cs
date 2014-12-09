using System;
using System.Security.Cryptography;
using System.Collections;
using System.Text;

namespace com.paypal.sdk.core
{
    public class OauthSignature
    {

       private static  String PARAM_DELIMETER = "&";
	   private static  String PARAM_SEPERATOR = "=";

        public enum HTTPMethod
        {
            GET, HEAD, POST, PUT, UPDATE
        };

	    /**
	     * Default Constructor
	     * 
	     * @param consumerKey
	     *            - Consumer key shared between PayPal and Consumer (OAuth
	     *            consumer)
	     * @param consumerSecret
	     *            - Secret shared between PayPal and Consumer (OAuth consumer)
	     */
        public OauthSignature(String consumerKey, byte[] consumerSecret)
        {
		    this.queryParams = new ArrayList();

		    this.consumerKey = consumerKey;
		    this.consumerSecret = consumerSecret;
		    this.httpMethod = "POST";
		   // this.signatureMethod = "HmacSHA1";
	    }

	    /**
	     * Sets Token to be used to generate signature.
	     * 
	     * @param token
	     *            - String version of Token. The token could be Access or
	     *            Request
	     */
	    public void setToken(String token) {
		    this.token = token;
	    }

	    /**
	     * Adds Parameter. Parameter could be part of URL, POST data.
	     * 
	     * @param name
	     *            parameter name with no URL encoding applied
	     * @param value
	     *            parameter value with no URL encoding applied
	     */
	    public void addParameter(String name, String value) {
		    queryParams.Add(new Parameter(name, value));
	    }

	    /**
	     * Sets Token secret as received in Token response.
	     * 
	     * @param secret
	     *            byte array of token secret
	     */
	    public void setTokenSecret(byte[] secret) {
		    this.tokenSecret = secret;
	    }

	    /**
	     * Sets URI for signature computation.
	     * 
	     * @param uri
	     *            - Script URI which will be normalized to
	     *            scheme://authority:port/path if not normalized already.
	     */
	    public void setRequestURI(String uri) {
		    requestURI = normalizeURI(uri);
	    }

	    /**
	     * Sets time stamp for signature computation.
	     * 
	     * @param timestamp
	     *            - time stamp at which Token request sends.
	     */
	    public void setTokenTimestamp(String timestamp) {
		    this.timestamp = timestamp;
	    }

	    /**
	     * Sets HTTP Method
	     * 
	     * @param method
	     *            HTTP method used for sending OAuth request
	     */
	    public void setHTTPMethod(HTTPMethod method) {
		    switch (method) {
            case HTTPMethod.GET:
			    httpMethod = "GET";
			    break;
		    case HTTPMethod.HEAD:
			    httpMethod = "HEAD";
			    break;
		    case HTTPMethod.PUT:
			    httpMethod = "PUT";
			    break;
		    case HTTPMethod.UPDATE:
			    httpMethod = "UPDATE";
			    break;
		    default:
			    httpMethod = "POST";
			    break;
		    }
	    }

	    /**
	     * Computes OAuth Signature as per OAuth specification using signature
	     * Method. using the specified encoding scheme {@code enc}.
	     * <p>
	     * 
	     * @return the Base64 encoded string.
	     * @throws OAuthException
	     *             if invalid arguments.
	     */
	    public string computeV1Signature() 
        {
		    if (consumerSecret == null || consumerSecret.Length == 0) {
			    throw new OAuthException("Consumer Serect or key not set.");
		    }

		    if (token == "" || tokenSecret.Length == 0 || requestURI == ""
				    || timestamp == "") {
			    throw new OAuthException(
					    "AuthToken or TokenSecret or Request URI or Timestap not set.");
		    }

		    string signature = "";
		    try {
                string consumerSec = System.Text.Encoding.GetEncoding("ASCII").GetString(consumerSecret);
                String key = PayPalURLEncoder.encode(consumerSec,"ASCII");
			    key += PARAM_DELIMETER;
                string tokenSec = System.Text.Encoding.GetEncoding("ASCII").GetString(tokenSecret);
                key += PayPalURLEncoder.encode(tokenSec,"ASCII");

			    ArrayList params1 = queryParams;

			    params1.Add(new Parameter("oauth_consumer_key", consumerKey));
			    params1.Add(new Parameter("oauth_version", "1.0"));
			    params1.Add(new Parameter("oauth_signature_method", "HMAC-SHA1"));
			    params1.Add(new Parameter("oauth_token", token));
			    params1.Add(new Parameter("oauth_timestamp", timestamp));

                params1.Sort();      

			    String signatureBase = this.httpMethod + PARAM_DELIMETER;

                signatureBase += PayPalURLEncoder.encode(requestURI, "ASCII");
			    signatureBase += PARAM_DELIMETER;

			    string paramString = "";
			    int Elements = params1.Count - 1;

                for (int counter = 0; counter <= Elements; counter++)
                {
                   Parameter current = (Parameter)params1[counter];

				    string ps = current.getName();
				    ps += PARAM_SEPERATOR;
				    ps += current.getValue();
				    if (counter<Elements)
					    ps += PARAM_DELIMETER;
				    paramString += ps;
			    }

			    signatureBase += PayPalURLEncoder.encode(paramString, "ASCII");

                Encoding encoding = System.Text.Encoding.ASCII;
                byte[] keyByte = encoding.GetBytes(key);

                HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);
                Encoding encoding1 = System.Text.Encoding.ASCII;
                byte[] SignBase = encoding1.GetBytes(signatureBase);
                byte[] digest = hmacsha1.ComputeHash(SignBase);

                signature = System.Convert.ToBase64String(digest);

		    } catch (Exception e) {
			    throw new OAuthException(e.Message,e);
		    }

		    return signature;
	    }

	    /**
	     * verifyV1Signature verifies signature against computed signature.
	     * 
	     * @return true if signature verified otherwise false
	     * @throws OAuthException
	     *             in case there are any failures in signature computation.
	     */
	    public Boolean verifyV1Signature(String signature)  {
		    string signatureComputed = computeV1Signature();
		    return signatureComputed != signature ? false : true;
	    }

	    /**
	     * normalizeURI normalizes the given URI as per OAuth spec
	     * 
	     * @param uri
	     * @return normalized URI. URI normalized to scheme://authority:port/path
	     * @throws OAuthException
	     */
	    private string normalizeURI(string uri) {
		    string normalizedURI = "", port = "", scheme = "", path = "", authority = "";
		    int i, j, k;

            try
            {
                i = uri.IndexOf(":");
                if (i == -1)
                    throw new OAuthException("Invalid URI.");
                else
                    scheme = uri.Substring(0, i);

                // find next : in URL
                j = uri.IndexOf(":", i + 2);
                if (j != -1)
                {
                    // port has specified in URI
                    authority = uri.Substring(scheme.Length + 3, (j-(scheme.Length + 3)));
                    k = uri.IndexOf("/", j);
                    if (k != -1)
                        port = uri.Substring(j + 1, (k-(j+1)));
                    else
                        port = uri.Substring(j + 1);
                }
                else
                {
                    // no port specified in uri
                    k = uri.IndexOf("/", scheme.Length + 3);
                    if (k != -1)
                        authority = uri.Substring(scheme.Length + 3, (k - (scheme.Length + 3)));
                    else
                        authority = uri.Substring(scheme.Length + 3);
                }

                if (k != -1)
                    path = uri.Substring(k);

                normalizedURI = scheme.ToLower();
                normalizedURI += "://";
                normalizedURI += authority.ToLower();

                if (scheme != null && port.Length > 0)
                {
                    if (scheme.Equals("http") && Convert.ToInt32(port) != 80)
                    {
                        normalizedURI += ":";
                        normalizedURI += port;
                    }
                    else if (scheme.Equals("https") && Convert.ToInt32(port) != 443)
                    {
                        normalizedURI += ":";
                        normalizedURI += port;
                    }
                }

            }
            catch (FormatException nfe)
            {
                throw new OAuthException("Invalid URI.", nfe);
            }
            catch (ArgumentOutOfRangeException are)
            {
                throw new OAuthException("Out Of Range.", are);
            }
		    normalizedURI += path;

		    return normalizedURI;
	    }

//	    private string signatureMethod;
	    private string consumerKey;
	    private string token;
	    private byte[] consumerSecret;
	    private byte[] tokenSecret;
	    private string requestURI;
	    private string timestamp;
	    private string httpMethod;
	    private ArrayList queryParams;

	    /**
	     * Inner class for representing Parameter
	     * 
	     */
        private class Parameter : System.IComparable
        {

		    public Parameter(String name, String value) {
			    this.m_name = name;
			    this.m_value = value;
		    }

		    public void setName(String name) {
			    this.m_name = name;
		    }

		    public void setValue(String val) {
			    this.m_value = val;
		    }

		    public String getName() {
			    return this.m_name;
		    }

		    public String getValue() {
			    return this.m_value;
		    }

		    private String m_name;
		    private String m_value;

            public int CompareTo(Object obj)
            {
                if (!(obj is Parameter))
                    throw new InvalidCastException("This object is not of type Parameter");

                Parameter param = (Parameter)obj;
                int retval = 0;
                if (param != null)
                {
                    retval = this.m_name.CompareTo(param.getName());
                    // if parameter names are equal then compare parameter values.
                    if (retval == 0)
                        retval = this.m_value.CompareTo(param.getValue());
                }
                return retval;
            }
	    }

        public static string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

	/**
	 * getAuthHeader accepts the required parameters and Provides OAuth signature and TimeStamp.
	 * 
	 * @param apiUserName
	 *                   API User name.
	 * @param apiPsw
	 *               	 API Password of user.
	 * @param accessToken
	 * 					 Obtained during Permission Request of token.	
	 * @param tokenSecret
	 * 					 Obtained during Permission Request of token.
	 * @param httpMethod
	 *                   HTTP Method (GET,POST etc.) 
	 * @param scriptURI
	 *                   API Server End Point.
	 * @param queryParams
	 *                   Extra 'name/value' parameters if required.
	 * @return
	 */
	    public static Hashtable getAuthHeader(String apiUserName, String apiPsw,
			    String accessToken, String tokenSecret, HTTPMethod httpMethod,
			    String scriptURI,Hashtable queryParams)
        {

		    Hashtable headers=new Hashtable();
		    String consumerKey = apiUserName;
		    String consumerSecretStr = apiPsw;
            byte[] consumerSecret = System.Text.Encoding.ASCII.GetBytes(consumerSecretStr);
		    String tokSecretStr = tokenSecret;
		    byte[] tokSecret = System.Text.Encoding.ASCII.GetBytes(tokSecretStr);
		    String authToken = accessToken;
		    String uri = scriptURI;

            string time = GenerateTimeStamp();

            OauthSignature oauth = new OauthSignature(consumerKey, consumerSecret);
			if(httpMethod == HTTPMethod.GET && queryParams != null) {		      
                  foreach(string name in queryParams.Keys){
                      oauth.addParameter(name,(string)queryParams[name]);
                  }
			  }	
		    oauth.setToken(authToken);

		    oauth.setHTTPMethod(httpMethod);

		    oauth.setTokenSecret(tokSecret);
		    oauth.setTokenTimestamp(time);
		    oauth.setRequestURI(uri);

		    //Compute Signature
		    String sig = oauth.computeV1Signature();
    		Console.WriteLine("Signature"+sig);
            Console.WriteLine("TimeStamp"+time);

            headers.Add("Signature", sig);
            headers.Add("TimeStamp", time);

		    return headers;

	    }

    }
}
