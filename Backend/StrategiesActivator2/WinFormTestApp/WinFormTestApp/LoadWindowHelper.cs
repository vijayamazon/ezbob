using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace WinFormTestApp
{
    public class LoadWindowHelper
    {
        public static List<Endpoint> LoadEndpointsFromConfig()
        {
            return LoadWindowHelper.LoadEndpointsFromConfig(null);
        }

        public static List<Endpoint> LoadEndpointsFromConfig(string configPath)
        {
            List<Endpoint> list = new List<Endpoint>();
            try
            {
                if (configPath == null)
                {
                    var configSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
                    ChannelEndpointElementCollection endpointElCollection = configSection.ElementInformation.Properties[string.Empty].Value as ChannelEndpointElementCollection;
                    foreach (ChannelEndpointElement element in endpointElCollection)
                    {
                        list.Add(new Endpoint() {
                            EndPointName = element.Name, 
                            EndPointPath = element.Address, 
                            BindingName = element.BindingConfiguration
                        });
                        
                    }
                }
                else
                {
                }
            }
            catch
            {
                throw new Exception("Cannot load endpoints! Please check *.config file.");
            }
            return list;
        }
    }
}
