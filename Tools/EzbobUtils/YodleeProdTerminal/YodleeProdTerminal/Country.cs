using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

namespace com.yodlee.sampleapps.helper
{
    class Country 
    {
         /**
     * Represents the United States.
     */
    public static  Country US = new Country(1, "US");

    /**
     * Represents Great Britain.
     */
    public static  Country GB = new Country(2, "GB");

    /**
     * Represents Australia.
     */
    public static  Country AU = new Country(3, "AU");

    /**
     * Represents Belgium.
     */
    public static  Country BE = new Country(4, "BE");

    /**
     * Represents China.
     */
    public static  Country CN = new Country(5, "CN");
    /**
     * Represents India.
     */
    public static  Country IN = new Country(6, "IN");
    /**
     * Represents Canada.
     */
    public static  Country CA = new Country(7, "CA");

    /**
     * Represents Spain.
     */
    public static  Country ES = new Country(8, "ES");

    protected static  Country[] countryArray = {US, GB, AU, BE, CN ,IN, CA, ES};
    protected static Hashtable countries = new Hashtable();
    protected static Hashtable countriesByCountryId = new Hashtable();

    static Country(){
        for (int i = 0; i < countryArray.Length; i++) {
            countries.Add(countryArray[i].getCountryCode(), countryArray[i]);
            countriesByCountryId.Add(countryArray[i].countryId, countryArray[i]);
        }
    }
    protected String countryCode;
    protected long countryId;

    private static  String COUNTRY_CODE = "countryCode";

    protected Country(long countryId, String countryCode) {
        this.countryId = countryId;
        this.countryCode = countryCode;
    }

    /**
     * Returns the country code.
     * <p>
     * @return  the country code
     */
    public String getCountryCode() {
        return countryCode;
    }

    public long getCountryId() {
        return countryId;
    }
/*
    public static Country getInstance(String countryCode) {
        if (countryCode == null) {
            return null;
        }
        Country country = (Country) countries.get(countryCode);
        return country;
    }*/

  /*  public static Country getInstance(Long countryId) {
        if (countryId == null) {
            return null;
        }
        return (Country) countriesByCountryId.get(countryId);
    }*/

    /**
     * Returns the country code. This returns the same value as the
     * {@link #getCountryCode <code>getCountryCode</code>} method.
     * <p>
     * @return  the country code
     */
    public String toString() {
        return countryCode;
    }
/*
    public bool equals(Object obj) {
        if (obj is Country) {
            if (countryCode == null) {
                return false;
            }
            Country country = (Country) obj;
            return countryCode.Equals(country.countryCode);
        } else {
            return false;
        }
    }*/

    /**
     * <b>For internal use only</b>.
     * <p>
     * Returns the fields for marshalling purposes.
     * Converts all primitives to equivalent Objects. Ex: int to Integer etc.
     * <p>
     * @return The fields to be marshalled in a map.
     */
    public Hashtable getFieldsForMarshalling() {
        Hashtable map = new Hashtable();

        map.Add(COUNTRY_CODE, countryCode);

        return map;
    }

    /**
     * <b>For internal use only</b>.
     * <p>
     * Creates an object of this class from a field map.
     * <p>
     * @param map the fields from which the object is to be created.
     * @return    the object created.
     */
 /*   public static Country createFromFields(Map map) {
        return getInstance((String) map.get(COUNTRY_CODE));
    }*/
    public Country(): base()
    {
        //super();
    }
   /* public void setCountryCode ( java.lang.String countryCode) { 
    this.countryCode = countryCode;
    }*/ 
    public void setCountryId ( long countryId) { 
    this.countryId = countryId;
    } 

    }
}
