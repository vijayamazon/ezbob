using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.Common;
using PayPal.Services.Private.Permissions;
using log4net;
using Permissions = PayPal.Platform.SDK.Permissions;

namespace EzBob.PayPalServiceLib
{
	public class PayPalPermissionServiceHelper : PayPalServiceWrapperBase
	{
		private static readonly ILog _log = LogManager.GetLogger( typeof( PayPalPermissionServiceHelper ) );

		private PayPalPermissionServiceHelper(IPayPalConfig config)
			:base(config, PayPalServiceType.Permissions)
		{
		}

		protected override string Endpoint
		{
			get { return Permissions.Endpoint; }
		}

		public static PayPalRermissionsGranted GetAccessToken( IPayPalConfig config, string requestToken, string verificationCode )
		{
			return new PayPalPermissionServiceHelper(config).GetAccessTokenInternal( requestToken, verificationCode );
		}

		private PayPalRermissionsGranted GetAccessTokenInternal( string requestToken, string verificationCode )
		{
			var getAccessTokenRequest = new GetAccessTokenRequest
			                            	{
			                            		requestEnvelope = GetRequestEnvelope(),
			                            		token = requestToken,
			                            		verifier = verificationCode
			                            	};


			var per = InternalCreateService();

			var response = per.getAccessToken( getAccessTokenRequest );

			if ( per.isSuccess.ToUpper() == "FAILURE" )
			{
				throw new Exception( "Get access token failed" );
			}

			return new PayPalRermissionsGranted
			       	{
			       		AccessToken = response.token,
			       		TokenSecret = response.tokenSecret,
			       		RequestToken = requestToken,
			       		VerificationCode = verificationCode,
			       	};
		}

		public static string GetRequestPermissionsUrl( IPayPalConfig config, string callback )
		{
			return new PayPalPermissionServiceHelper(config).GetRequestPermissionsUrlInternal( callback );
		}

		private string GetRequestPermissionsUrlInternal( string callback )
		{
			var per = InternalCreateService();

			var permissionsRequest = new RequestPermissionsRequest
			                         	{
			                         		requestEnvelope = GetRequestEnvelope(),
			                         		callback = callback,
			                         		scope = new[]
			                         		        	{
			                         		        		"ACCESS_BASIC_PERSONAL_DATA",
			                         		        		"ACCESS_ADVANCED_PERSONAL_DATA",
			                         		        		"TRANSACTION_SEARCH",
			                         		        		"TRANSACTION_DETAILS"
			                         		        	}
			                         	};
			
			
			
			var pResponse = per.requestPermissions( permissionsRequest );

			if ( per.isSuccess.ToUpper() == "FAILURE" )
			{
				_log.Error( "requestPermissions failed" );
				_log.Error( per.LastError.ErrorDetails );
				throw new PayPalException( per.LastError.ErrorDetails );
			}

			return ConnectionInfo.RedirectUrl + "_grant-permission&request_token=" + pResponse.token;
		}

		private Permissions InternalCreateService( PayPalRermissionsGranted securityData = null, string scriptName = null )
		{
			return new Permissions { APIProfile = GetProfile(securityData, scriptName) };
		}

		public static PayPalPersonalData GetAccountInfo(IPayPalConfig config, PayPalRermissionsGranted securityData)
		{
			return new PayPalPermissionServiceHelper(config).InternalGetAccountInfo(securityData);
		}

		private PayPalPersonalData InternalGetAccountInfo( PayPalRermissionsGranted securityData )
		{
			var per = InternalCreateService( securityData, "GetAdvancedPersonalData" );

			var getAdvancedPersonalDataRequest = new GetAdvancedPersonalDataRequest
			{
				requestEnvelope = GetRequestEnvelope(),
				attributeList = Enum.GetValues( typeof( PersonalAttribute ) ).OfType<PersonalAttribute>().ToArray()
			};


			var responseAdvanced = per.getAdvancedPersonalData( getAdvancedPersonalDataRequest );
			if ( per.isSuccess.ToUpper() == "FAILURE" )
			{
				_log.Error( "requestPermissions failed" );
				_log.Error( per.LastError.ErrorDetails );
				throw new PayPalException( per.LastError.ErrorDetails );
			}

			var dict = responseAdvanced.response.ToDictionary( k => k.personalDataKey, v => v.personalDataValue );

			DateTime submitDate = responseAdvanced.responseEnvelope.timestamp.ToUniversalTime();

			return ParcePersonalData( dict, submitDate );		
		}

		private PayPalPersonalData ParcePersonalData( Dictionary<PersonalAttribute, string> dict, DateTime submitDate )
		{
			var personalData = new PayPalPersonalData { SubmittedDate = submitDate };

			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgnamePersonfirst, v => personalData.FirstName = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgnamePersonlast, v => personalData.LastName = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactemail, v => personalData.Email = v );
			TryGetDictValue( dict, PersonalAttribute.httpschemaopenidnetcontactfullname, v => personalData.FullName = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcompanyname, v => personalData.BusinessName = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactcountryhome, v => personalData.AddressCountry = v );
			TryGetDictValue( dict, PersonalAttribute.httpswwwpaypalcomwebappsauthschemapayerID, v => personalData.PlayerId = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgbirthDate, v => personalData.BirthDate = ConvertToDateTime( v ) );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactpostalCodehome, v => personalData.AddressPostCode = v );
			TryGetDictValue( dict, PersonalAttribute.httpschemaopenidnetcontactstreet1, v => personalData.AddressStreet1 = v );
			TryGetDictValue( dict, PersonalAttribute.httpschemaopenidnetcontactstreet2, v => personalData.AddressStreet2 = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactcityhome, v => personalData.AddressCity = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactstatehome, v => personalData.AddressState = v );
			TryGetDictValue( dict, PersonalAttribute.httpaxschemaorgcontactphonedefault, v => personalData.Phone = v );

			return personalData;
		}

		private DateTime? ConvertToDateTime( string str )
		{
			// 19820802
			var culture = CultureInfo.InvariantCulture;
			DateTime dateTime;
			if ( DateTime.TryParseExact( str, "yyyyMMdd", culture, DateTimeStyles.None, out dateTime ) )
			{
				return dateTime;
			}

			return null;
		}

		private void TryGetDictValue( IDictionary<PersonalAttribute, string> dict, PersonalAttribute key, Action<string> action )
		{
			string val;
			if ( dict.TryGetValue( key, out val ) )
			{
				action( val );
			}
		}

		/// <summary>
		/// Returns the default request envelope object 
		/// </summary>
		/// <returns>RequestEnvelope</returns>
		private static RequestEnvelope GetRequestEnvelope()
		{
			return new RequestEnvelope { errorLanguage = "en_US" };

		}
	}
}