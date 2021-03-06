﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.yodlee.sampleapps.datatypes
{
    class Currency
    {
        public const string AED = "AED";
        public const string AFA = "AFA";
        public const string ALL = "ALL";
        public const string AMD = "AMD";
        public const string ANG = "ANG";
        public const string AOA = "AOA";
        public const string ARS = "ARS";
        public const string AUD = "AUD";
        public const string AWG = "AWG";
        public const string AZM = "AZM";
        public const string BAM = "BAM";
        public const string BBD = "BBD";
        public const string BDT = "BDT";
        public const string BGL = "BGL";
        public const string BHD = "BHD";
        public const string BIF = "BIF";
        public const string BMD = "BMD";
        public const string BND = "BND";
        public const string BOB = "BOB";
        public const string BRL = "BRL";
        public const string BSD = "BSD";
        public const string BTN = "BTN";
        public const string BWP = "BWP";
        public const string BYR = "BYR";
        public const string BZD = "BZD";
        public const string CAD = "CAD";
        public const string CDF = "CDF";
        public const string CHF = "CHF";
        public const string CLP = "CLP";
        public const string CNY = "CNY";
        public const string COP = "COP";
        public const string CRC = "CRC";
        public const string CUP = "CUP";
        public const string CVE = "CVE";
        public const string CYP = "CYP";
        public const string CZK = "CZK";
        public const string DJF = "DJF";
        public const string DKK = "DKK";
        public const string DOP = "DOP";
        public const string DZD = "DZD";
        public const string EEK = "EEK";
        public const string EGP = "EGP";
        public const string ERN = "ERN";
        public const string ETB = "ETB";
        public const string EUR = "EUR";
        public const string FJD = "FJD";
        public const string FKP = "FKP";
        public const string GBP = "GBP";
        public const string GEL = "GEL";
        public const string GGP = "GGP";
        public const string GHC = "GHC";
        public const string GIP = "GIP";
        public const string GMD = "GMD";
        public const string GNF = "GNF";
        public const string GTQ = "GTQ";
        public const string GYD = "GYD";
        public const string HKD = "HKD";
        public const string HNL = "HNL";
        public const string HRK = "HRK";
        public const string HTG = "HTG";
        public const string HUF = "HUF";
        public const string IDR = "IDR";
        public const string ILS = "ILS";
        public const string IMP = "IMP";
        public const string INR = "INR";
        public const string IQD = "IQD";
        public const string IRR = "IRR";
        public const string ISK = "ISK";
        public const string JEP = "JEP";
        public const string JMD = "JMD";
        public const string JOD = "JOD";
        public const string JPY = "JPY";
        public const string KES = "KES";
        public const string KGS = "KGS";
        public const string KHR = "KHR";
        public const string KMF = "KMF";
        public const string KPW = "KPW";
        public const string KRW = "KRW";
        public const string KWD = "KWD";
        public const string KYD = "KYD";
        public const string KZT = "KZT";
        public const string LAK = "LAK";
        public const string LBP = "LBP";
        public const string LKR = "LKR";
        public const string LRD = "LRD";
        public const string LSL = "LSL";
        public const string LTL = "LTL";
        public const string LVL = "LVL";
        public const string LYD = "LYD";
        public const string MAD = "MAD";
        public const string MDL = "MDL";
        public const string MGF = "MGF";
        public const string MKD = "MKD";
        public const string MMK = "MMK";
        public const string MNT = "MNT";
        public const string MOP = "MOP";
        public const string MRO = "MRO";
        public const string MTL = "MTL";
        public const string MUR = "MUR";
        public const string MVR = "MVR";
        public const string MWK = "MWK";
        public const string MXN = "MXN";
        public const string MYR = "MYR";
        public const string MZM = "MZM";
        public const string NAD = "NAD";
        public const string NGN = "NGN";
        public const string NIO = "NIO";
        public const string NOK = "NOK";
        public const string NPR = "NPR";
        public const string NZD = "NZD";
        public const string OMR = "OMR";
        public const string PAB = "PAB";
        public const string PEN = "PEN";
        public const string PGK = "PGK";
        public const string PHP = "PHP";
        public const string PKR = "PKR";
        public const string PLN = "PLN";
        public const string PYG = "PYG";
        public const string QAR = "QAR";
        public const string ROL = "ROL";
        public const string RUR = "RUR";
        public const string RWF = "RWF";
        public const string SAR = "SAR";
        public const string SBD = "SBD";
        public const string SCR = "SCR";
        public const string SDD = "SDD";
        public const string SEK = "SEK";
        public const string SGD = "SGD";
        public const string SHP = "SHP";
        public const string SIT = "SIT";
        public const string SKK = "SKK";
        public const string SLL = "SLL";
        public const string SOS = "SOS";
        public const string SPL = "SPL";
        public const string SRG = "SRG";
        public const string STD = "STD";
        public const string SVC = "SVC";
        public const string SYP = "SYP";
        public const string SZL = "SZL";
        public const string THB = "THB";
        public const string TJR = "TJR";
        public const string TMM = "TMM";
        public const string TND = "TND";
        public const string TOP = "TOP";
        public const string TRL = "TRL";
        public const string TTD = "TTD";
        public const string TVD = "TVD";
        public const string TWD = "TWD";
        public const string TZS = "TZS";
        public const string UAH = "UAH";
        public const string UGX = "UGX";
        public const string USD = "USD";
        public const string UYU = "UYU";
        public const string UZS = "UZS";
        public const string VEB = "VEB";
        public const string VND = "VND";
        public const string VUV = "VUV";
        public const string WST = "WST";
        public const string XAF = "XAF";
        public const string XAG = "XAG";
        public const string XAU = "XAU";
        public const string XCD = "XCD";
        public const string XDR = "XDR";
        public const string XOF = "XOF";
        public const string XPD = "XPD";
        public const string XPF = "XPF";
        public const string XPT = "XPT";
        public const string YER = "YER";
        public const string YUM = "YUM";
        public const string ZAR = "ZAR";
        public const string ZMK = "ZMK";
        public const string ZWD = "ZWD";
    }
}
