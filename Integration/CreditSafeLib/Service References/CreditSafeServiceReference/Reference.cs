﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ezbob.CreditSafeLib.CreditSafeServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="https://www.creditsafeuk.com/getdatuk/service/", ConfigurationName="CreditSafeServiceReference.CreditsafeServicesSoap")]
    public interface CreditsafeServicesSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="https://www.creditsafeuk.com/getdatuk/service/GetData", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string GetData(string requestXmlStr);
        
        [System.ServiceModel.OperationContractAttribute(Action="https://www.creditsafeuk.com/getdatuk/service/GetData", ReplyAction="*")]
        System.Threading.Tasks.Task<string> GetDataAsync(string requestXmlStr);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface CreditsafeServicesSoapChannel : Ezbob.CreditSafeLib.CreditSafeServiceReference.CreditsafeServicesSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class CreditsafeServicesSoapClient : System.ServiceModel.ClientBase<Ezbob.CreditSafeLib.CreditSafeServiceReference.CreditsafeServicesSoap>, Ezbob.CreditSafeLib.CreditSafeServiceReference.CreditsafeServicesSoap {
        
        public CreditsafeServicesSoapClient() {
        }
        
        public CreditsafeServicesSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CreditsafeServicesSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CreditsafeServicesSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CreditsafeServicesSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetData(string requestXmlStr) {
            return base.Channel.GetData(requestXmlStr);
        }
        
        public System.Threading.Tasks.Task<string> GetDataAsync(string requestXmlStr) {
            return base.Channel.GetDataAsync(requestXmlStr);
        }
    }
}
