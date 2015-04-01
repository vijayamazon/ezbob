namespace EzBob.Web.Areas.Underwriter.Models.Fraud {
    using System;
    using System.Collections.Generic;
    
    public class IovationDetailModel {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class IovationDetailsModel {
        public int Id { get; set; }
        public string Result { get; set; }
        public string Reason { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime Created { get; set; }
        public string Origin { get; set; }
        public IEnumerable<IovationDetailModel> Details { get; set; }

        public void DetailsNamesToDescription(IEnumerable<IovationDetailModel> details) {
            foreach (var detail in details) {
                switch (detail.name) {
                    case "device.alias":
                        detail.name = "iovation identifier for the device";
                        break;
                    case "device.firstseen":
                        detail.name = "date/time the device was first seen by iovation.";
                        break;
                    case "device.new":
                        detail.name = "device has never been seen by iovation";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.screen":
                        detail.name = "Screen resolution of the device";
                        break;
                    case "device.type":
                        detail.name = "Type of device";
                        break;
                    case "device.os":
                        detail.name = "Operating system version of the device.";
                        break;
                    case "device.tz":
                        detail.name = "Browser time zone";
                        break;
                    case "device.js.enabled":
                        detail.name = "JavaScript is enabled";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.flash.enabled":
                        detail.name = "Flash is enabled";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.flash.installed":
                        detail.name = "Flash is installed";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.flash.version":
                        detail.name = "Version of Flash installed";
                        break;
                    case "device.flash.storage.enabled":
                        detail.name = "Flash local storage is enabled";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.cookie.enabled":
                        detail.name = "JavaScript cookies are enabled";
                        detail.value = detail.value == "1" ? "Yes" : "No";
                        break;
                    case "device.browser.type":
                        detail.name = "Browser type";
                        break;
                    case "device.browser.version":
                        detail.name = "Browser version";
                        break;
                    case "device.browser.charset":
                        detail.name = "Browser character set";
                        break;
                    case "device.browser.configuredlang":
                        detail.name = "Language set the browser will accept.";
                        break;
                    case "device.browser.lang":
                        detail.name = "Browser compilation language";
                        break;
                    case "device.trustScore":
                        detail.name = "Number iovation TrustScore for the device";
                        break;
                    case "ipaddress":
                        detail.name = "IP address provided in the request";
                        break;
                    case "ipaddress.org":
                        detail.name = "Organization the IP address is assigned to";
                        break;
                    case "ipaddress.isp":
                        detail.name = "Internet service provider of the IP address";
                        break;
                    case "ipaddress.proxy":
                        detail.name = "Indicator or special attributes for the IP address";
                        break;
                    case "ipaddress.loc.lat":
                        detail.name = "Decimal Latitude associated with the IP address";
                        break;
                    case "ipaddress.loc.lng":
                        detail.name = "Decimal Longitude associated with the IP address";
                        break;
                    case "ipaddress.loc.city":
                        detail.name = "City associated with the IP address";
                        break;
                    case "ipaddress.loc.countrycode":
                        detail.name = "Country code associated with the IP address";
                        break;
                    case "ipaddress.loc.country":
                        detail.name = "Country associated with the IP address";
                        break;
                    case "ipaddress.loc.region":
                        detail.name = "State/region name associated with the IP address";
                        break;
                    case "realipaddress":
                        detail.name = "The Real IP address determined for the request";
                        break;
                    case "realipaddress.source":
                        detail.name = "Flag indicating the source of the Real IP address";
                        break;
                    case "realipaddress.org":
                        detail.name = "Organization the Real IP address is assigned to";
                        break;
                    case "realipaddress.isp":
                        detail.name = "Internet service provider of the Real IP address";
                        break;
                    case "realipaddress.proxy":
                        detail.name = "Indicator or special attributes for the Real IP address";
                        break;
                    case "realipaddress.loc.lat":
                        detail.name = "Decimal Latitude associated with the Real IP address";
                        break;
                    case "realipaddress.loc.lng":
                        detail.name = "Decimal Longitude associated with the Real IP address";
                        break;
                    case "realipaddress.loc.city":
                        detail.name = "City associated with the Real IP address";
                        break;
                    case "realipaddress.loc.countrycode":
                        detail.name = "Country code associated with the Real IP address";
                        break;
                    case "realipaddress.loc.country":
                        detail.name = "Country associated with the Real IP address";
                        break;
                    case "realipaddress.loc.region":
                        detail.name = "State/region name associated with the Real IP address";
                        break;
                    case "ruleset.rulesmatched":
                        detail.name = "Number of rules that matched";
                        break;
                    case "ruleset.score":
                        detail.name = "Combined weight of all matching rules";
                        break;
                    default:
                        if (detail.name.EndsWith("].reason")) {
                            detail.name = detail.name + " The reason associated with the rule that matched";
                            break;
                        }
                        if (detail.name.EndsWith("].score")) {
                            detail.name = detail.name + " Score contribution from the matched rule";
                            break;
                        }
                        if (detail.name.EndsWith("].type")) {
                            detail.name = detail.name + " Type of rule that matched";
                        }
                        break;
                }
                
            }
        }
    }
}