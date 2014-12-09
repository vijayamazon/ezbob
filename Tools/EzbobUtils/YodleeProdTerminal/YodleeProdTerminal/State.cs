using System;
using System.Xml.Schema;
using System.Xml.Serialization;
namespace com.yodlee.sampleapps
{

    public class State
    {

        public const string AA = "AA";

        public const long AA_CODE = 1;

        public const string AE = "AE";

        public const long AE_CODE = 2;

        public const string AK = "AK";

        public const long AK_CODE = 4;

        public const string AL = "AL";

        public const long AL_CODE = 3;

        public const string ALABAMA_STRING = "ALABAMA";

        public const string ALASKA_STRING = "ALASKA";

        public const string AMERICAN_SAMOA_STRING = "AMERICAN_SAMOA";

        public const string AP = "AP";

        public const long AP_CODE = 62;

        public const string AR = "AR";

        public const long AR_CODE = 7;

        public const string ARIZONA_STRING = "ARIZONA";

        public const string ARKANSAS_STRING = "ARKANSAS";

        public const string ARMED_FORCES_AMERICAS_EXCEPT_CANADA_STRING = "ARMED FORCES AMERICAS (EXCEPT CANADA)";

        public const string ARMED_FORCES_EUROPE_AFRICA_CANADA_STRING = "ARMED_FORCES_EUROPE_AFRICA_CANADA";

        public const string ARMED_FORCES_PACIFIC_STRING = "ARMED_FORCES_PACIFIC";

        public const string AS = "AS";

        public const long AS_CODE = 5;

        public const string AZ = "AZ";

        public const long AZ_CODE = 6;

        public const string CA = "CA";

        public const long CA_CODE = 8;

        public const string CALIFORNIA_STRING = "CALIFORNIA";

        public const string CO = "CO";

        public const long CO_CODE = 9;

        public const string COLORADO_STRING = "COLORADO";

        public const string CONNECTICUT_STRING = "CONNECTICUT";

        public const string CT = "CT";

        public const long CT_CODE = 10;

        public const string DC = "DC";

        public const long DC_CODE = 12;

        public const string DE = "DE";

        public const long DE_CODE = 11;

        public const string DELAWARE_STRING = "DELAWARE";

        public const string DISTRICT_OF_COLUMBIA_STRING = "DISTRICT_OF_COLUMBIA";

        public const string FEDERATED_STATES_OF_MICRONESIA_STRING = "FEDERATED_STATES_OF_MICRONESIA";

        public const string FL = "FL";

        public const long FL_CODE = 14;

        public const string FLORIDA_STRING = "FLORIDA";

        public const string FM = "FM";

        public const long FM_CODE = 13;

        public const string GA = "GA";

        public const long GA_CODE = 15;

        public const string GEORGIA_STRING = "GEORGIA";

        public const string GU = "GU";

        public const long GU_CODE = 16;

        public const string GUAM_STRING = "GUAM";

        public const string HAWAII_STRING = "HAWAII";

        public const string HI = "HI";

        public const long HI_CODE = 17;

        public const string IA = "IA";

        public const long IA_CODE = 21;

        public const string ID = "ID";

        public const long ID_CODE = 18;

        public const string IDAHO_STRING = "IDAHO";

        public const string IL = "IL";

        public const long IL_CODE = 19;

        public const string ILLINOIS_STRING = "ILLINOIS";

        public const string IN = "IN";

        public const long IN_CODE = 20;

        public const string INDIANA_STRING = "INDIANA";

        public const string IOWA_STRING = "IOWA";

        public const string KANSAS_STRING = "KANSAS";

        public const string KENTUCKY_STRING = "KENTUCKY";

        public const string KS = "KS";

        public const long KS_CODE = 22;

        public const string KY = "KY";

        public const long KY_CODE = 23;

        public const string LA = "LA";

        public const long LA_CODE = 24;

        public const string LOUISIANA_STRING = "LOUISIANA";

        public const string MA = "MA";

        public const long MA_CODE = 28;

        public const string MAINE_STRING = "MAINE";

        public const string MARSHALL_ISLANDS_STRING = "MARSHALL_ISLANDS";

        public const string MARYLAND_STRING = "MARYLAND";

        public const string MASSACHUSETTS_STRING = "MASSACHUSETTS";

        public const string MD = "MD";

        public const long MD_CODE = 27;

        public const string ME = "ME";

        public const long ME_CODE = 25;

        public const string MH = "MH";

        public const long MH_CODE = 26;

        public const string MI = "MI";

        public const long MI_CODE = 29;

        public const string MICHIGAN_STRING = "MICHIGAN";

        public const string MINNESOTA_STRING = "MINNESOTA";

        public const string MISSISSIPPI_STRING = "MISSISSIPPI";

        public const string MISSOURI_STRING = "MISSOURI";

        public const string MN = "MN";

        public const long MN_CODE = 30;

        public const string MO = "MO";

        public const long MO_CODE = 32;

        public const string MONTANA_STRING = "MONTANA";

        public const string MP = "MP";

        public const long MP_CODE = 42;

        public const string MS = "MS";

        public const long MS_CODE = 31;

        public const string MT = "MT";

        public const long MT_CODE = 33;

        public const string NC = "NC";

        public const long NC_CODE = 40;

        public const string ND = "ND";

        public const long ND_CODE = 41;

        public const string NE = "NE";

        public const long NE_CODE = 34;

        public const string NEBRASKA_STRING = "NEBRASKA";

        public const string NEVADA_STRING = "NEVADA";

        public const string NEW_HAMPSHIRE_STRING = "NEW_HAMPSHIRE";

        public const string NEW_JERSEY_STRING = "NEW_JERSEY";

        public const string NEW_MEXICO_STRING = "NEW_MEXICO";

        public const string NEW_YORK_STRING = "NEW_YORK";

        public const string NH = "NH";

        public const long NH_CODE = 36;

        public const string NJ = "NJ";

        public const long NJ_CODE = 37;

        public const string NM = "NM";

        public const long NM_CODE = 38;

        public const string NORTH_CAROLINA_STRING = "NORTH_CAROLINA";

        public const string NORTH_DAKOTA_STRING = "NORTH_DAKOTA";

        public const string NORTHERN_MARIANA_ISLANDS_STRING = "NORTHERN_MARIANA_ISLANDS";

        public const string NV = "NV";

        public const long NV_CODE = 35;

        public const string NY = "NY";

        public const long NY_CODE = 39;

        public const string OH = "OH";

        public const long OH_CODE = 43;

        public const string OHIO_STRING = "OHIO";

        public const string OK = "OK";

        public const long OK_CODE = 44;

        public const string OKLAHOMA_STRING = "OKLAHOMA";

        public const string OR = "OR";

        public const long OR_CODE = 45;

        public const string OREGON_STRING = "OREGON";

        public const string PA = "PA";

        public const long PA_CODE = 47;

        public const string PALAU_STRING = "PALAU";

        public const string PENNSYLVANIA_STRING = "PENNSYLVANIA";

        public const string PR = "PR";

        public const long PR_CODE = 48;

        public const string PUERTO_RICO_STRING = "PUERTO_RICO";

        public const string PW = "PW";

        public const long PW_CODE = 46;

        public const string RHODE_ISLAND_STRING = "RHODE_ISLAND";

        public const string RI = "RI";

        public const long RI_CODE = 49;

        public const string SC = "SC";

        public const long SC_CODE = 50;

        public const string SD = "SD";

        public const long SD_CODE = 51;

        public const string SOUTH_CAROLINA_STRING = "SOUTH_CAROLINA";

        public const string SOUTH_DAKOTA_STRING = "SOUTH_DAKOTA";

        public const string TENNESSEE_STRING = "TENNESSEE";

        public const string TEXAS_STRING = "TEXAS";

        public const string TN = "TN";

        public const long TN_CODE = 52;

        public const string TX = "TX";

        public const long TX_CODE = 53;

        public const string UT = "UT";

        public const long UT_CODE = 54;

        public const string UTAH_STRING = "UTAH";

        public const string VA = "VA";

        public const long VA_CODE = 57;

        public const string VERMONT_STRING = "VERMONT";

        public const string VI = "VI";

        public const long VI_CODE = 56;

        public const string VIRGIN_ISLANDS_STRING = "VIRGIN_ISLANDS";

        public const string VIRGINIA_STRING = "VIRGINIA";

        public const string VT = "VT";

        public const long VT_CODE = 55;

        public const string WA = "WA";

        public const long WA_CODE = 58;

        public const string WASHINGTON_STRING = "WASHINGTON";

        public const string WEST_VIRGINIA_STRING = "WEST_VIRGINIA";

        public const string WI = "WI";

        public const long WI_CODE = 60;

        public const string WISCONSIN_STRING = "WISCONSIN";

        public const string WV = "WV";

        public const long WV_CODE = 59;

        public const string WY = "WY";

        public const long WY_CODE = 61;

        public const string WYOMING_STRING = "WYOMING";

        public string abbreviation;

        public static State_US ALABAMA;

        public static State_US ALASKA;

        public static State_US AMERICAN_SAMOA;

        public static State_US ARIZONA;

        public static State_US ARKANSAS;

        public static State_US ARMED_FORCES_AMERICAS_EXCEPT_CANADA;

        public static State_US ARMED_FORCES_EUROPE_AFRICA_CANADA;

        public static State_US ARMED_FORCES_PACIFIC;

        public static State_US CALIFORNIA;

        public static State_US COLORADO;

        public static State_US CONNECTICUT;

        public static State_US DELAWARE;

        public static State_US DISTRICT_OF_COLUMBIA;

        public static State_US FEDERATED_STATES_OF_MICRONESIA;

        public static State_US FLORIDA;

        public static State_US GEORGIA;

        public static State_US GUAM;

        public static State_US HAWAII;

        public static State_US IDAHO;

        public static State_US ILLINOIS;

        public static State_US INDIANA;

        public static State_US IOWA;

        public static State_US KANSAS;

        public static State_US KENTUCKY;

        public static State_US LOUISIANA;

        public static State_US MAINE;

        public static State_US MARSHALL_ISLANDS;

        public static State_US MARYLAND;

        public static State_US MASSACHUSETTS;

        public static State_US MICHIGAN;

        public static State_US MINNESOTA;

        public static State_US MISSISSIPPI;

        public static State_US MISSOURI;

        public static State_US MONTANA;

        public static State_US NEBRASKA;

        public static State_US NEVADA;

        public static State_US NEW_HAMPSHIRE;

        public static State_US NEW_JERSEY;

        public static State_US NEW_MEXICO;

        public static State_US NEW_YORK;

        public static State_US NORTH_CAROLINA;

        public static State_US NORTH_DAKOTA;

        public static State_US NORTHERN_MARIANA_ISLANDS;

        public static State_US OHIO;

        public static State_US OKLAHOMA;

        public static State_US OREGON;

        public static State_US PALAU;

        public static State_US PENNSYLVANIA;

        public static State_US PUERTO_RICO;

        public static State_US RHODE_ISLAND;

        public static State_US SOUTH_CAROLINA;

        public static State_US SOUTH_DAKOTA;

        public long stateId;

        public string stateName;

        public static State_US TENNESSEE;

        public static State_US TEXAS;

        public static State_US UTAH;

        public static State_US VERMONT;

        public static State_US VIRGIN_ISLANDS;

        public static State_US VIRGINIA;

        public static State_US WASHINGTON;

        public static State_US WEST_VIRGINIA;

        public static State_US WISCONSIN;

        public static State_US WYOMING;

    }

}
