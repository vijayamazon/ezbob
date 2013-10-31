using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace com.yodlee.sampleapps.helper
{
    class ContainerTypesHelper
    {
        public static String AIR_RESERVATION  = "travel";	
	 public static String AUCTION = "auction"; 
	 public static String BANK = "bank"; 
	 public static String BILL = "bills"; 
	 public static String BILL_PAY_SERVICE = "bill_payment"; 
	 public static String BOOKMARKLINK = "bookmarklink"; 
	 public static String CABLE_SATELLITE = "cable_satellite"; 
	 public static String CALENDAR = "calendar"; 
	 public static String CAR_RESERVATION = "rentals"; 
	 public static String CHARITIES = "charities"; 
	 public static String CHARTS = "charts"; 
	 public static String CHAT = "chats"; 
	 public static String CONSUMER_GUIDE = "consumer_guide"; 
	 public static String CREDIT_CARD = "credits"; 
	 public static String DEAL = "deal"; 
	 public static String HOTEL_RESERVATION = "hotel_reservations"; 
	 public static String INSURANCE = "insurance"; 
	 public static String INVESTMENT = "stocks"; 
	 public static String ISP = "isp"; 
	 public static String JOB = "jobs"; 
	 public static String LOAN = "loans"; 
	 public static String MAIL = "mail"; 
	 public static String MESSAGE_BOARD = "messageboards"; 
	 public static String MINUTES = "minutes"; 
	 public static String MISCELLANEOUS = "miscellaneous"; 
	 public static String MORTGAGE = "mortgage"; 
	 public static String NEWS = "news"; 
	 public static String ORDER = "orders"; 
	 public static String OTHER_ASSETS = "other_assets"; 
	 public static String OTHER_LIABILITIES = "other_liabilities"; 
	 public static String PREPAY = "prepay"; 
	 public static String REALESTATE = "RealEstate"; 
	 public static String RESERVATION = "reservations"; 
	 public static String REWARD_PROGRAM = "miles"; 
	 public static String TELEPHONE = "telephone"; 
	 public static String UTILITIES = "utilities"; 

	 protected static Hashtable containerTypes;
	 
	 static ContainerTypesHelper(){
	        containerTypes = new Hashtable(36);

	        containerTypes.Add(CREDIT_CARD, null);
	        containerTypes.Add(INVESTMENT, null);
	        containerTypes.Add(BANK, null);
	        containerTypes.Add(MINUTES, null);
	        containerTypes.Add(MAIL, null);
	        containerTypes.Add(ORDER, null);
	        containerTypes.Add(BILL, null);
	        containerTypes.Add(ISP, null);
	        containerTypes.Add(PREPAY, null);
	        containerTypes.Add(CHARITIES, null);   
	        containerTypes.Add(TELEPHONE, null);
	        containerTypes.Add(AIR_RESERVATION, null);
	        containerTypes.Add(CAR_RESERVATION, null);
	        containerTypes.Add(BILL_PAY_SERVICE, null);
	        containerTypes.Add(CALENDAR, null);
	        containerTypes.Add(CHAT, null);

	        containerTypes.Add(JOB, null);
	        containerTypes.Add(DEAL, null);
	        containerTypes.Add(MESSAGE_BOARD, null);

	        containerTypes.Add(CONSUMER_GUIDE, null);
	        containerTypes.Add(HOTEL_RESERVATION, null);
            containerTypes.Add(AUCTION, null);
            containerTypes.Add(LOAN, null);
            containerTypes.Add(MORTGAGE, null);
            containerTypes.Add(INSURANCE, null);
            containerTypes.Add(REWARD_PROGRAM, null);
            containerTypes.Add(NEWS, null);
            containerTypes.Add(RESERVATION, null);
            containerTypes.Add(CHARTS, null);
            containerTypes.Add(UTILITIES, null);
            containerTypes.Add(CABLE_SATELLITE, null);
            containerTypes.Add(MISCELLANEOUS, null);
            containerTypes.Add(OTHER_ASSETS, null);
            containerTypes.Add(OTHER_LIABILITIES, null);

            containerTypes.Add(BOOKMARKLINK, null);
            containerTypes.Add(REALESTATE, null);
	    }
	 
	    /**
	     * Returns true if the given container type is a valid one, otherwise false.
	     * @param containerType The container type whose validity is being checked.
	     * <p>
	     * @return true if the given container type is a valid one, otherwise false.
	     */
	    public static bool isValid(String containerType) {
	        //Check without using reflection.
	        if (containerTypes.ContainsKey(containerType)) {
	            return true;
	        }
	        return false;
	    }
    }
}
