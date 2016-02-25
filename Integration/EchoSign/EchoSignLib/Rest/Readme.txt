ECHOSIGN REST API uses OAUTH2 'Authorization code flow'.
This flow requires to have 'Client Id', 'Client Secret' and 'Redirect url'.
Having all above we need to obtain an authorization code (which is valid for 5 min)and having this code we should get the refresh token. 
We use the refresh token to get the 'access token' in order to be able to use the REST API. 

The authorization code (and refresh token obtained by it) depends on scope which determines which action are allowed.
So if we changes the scope we should get new refresh token.

We have a registered application in our echosign account.
1. Click on 'Adobe DC eSign API' -> 'Api Applications'
2. Click on our application -> 'Configure OAuth for Application'.
3. In a window you will see our application's 'Client Id' and 'Client Secret'. We should define the redirect url. (for example: https://redirect.ezbob.com).
   In this window we should define the scope. (When we will want to get authorization code we should ask for scope that we enabled)

   This is a possible url for authorization code request.
   https://secure.na1.echosign.com/public/oauth?redirect_uri=https://redirect.ezbob.com&response_type=code&client_id=96A2AM24676Z7M&scope=user_login:self+agreement_send:account+agreement_write:account+agreement_read:account+widget_read:account+widget_write:account+library_read:account+library_write:account

   Put this url in browser. You will get a dialog with a button (see Readme.png for example). Push the button and you will see in browser's navigation bar something like this:
   https://redirect.ezbob.com/?code=CBNCKBAAHBCAABAAQS0iZOJ0LRXP-KrjBTkM3fKiGFggpHgu

   after 'code=' is our authorization code.

4. When you have a code, use our rest client to get refresh token.

	This is an example of response: 

	{
		"expires_in":3600,
		"token_type":"Bearer",
		"refresh_token":"3AAABLblqZblablablabla",
		"access_token":"3AAABLblqZhAY-6tBhRh_blablablabla"
	}


	-------------------- CONFIGURATION VARIABLES -----------------------------
INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('EchoSignClientId', '96A2AM24676Z7M', 'EchoSign OAuth Client Id')
INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('EchoSignClientSecret', '98867629f92bfc28c8fd8df662d74626', 'EchoSign OAuth Client Secret')
INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('EchoSignRedirectUri', 'https://redirect.ezbob.com', 'Redirect uri specified in application configuration')
INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('EchoSignRefreshToken', '3AAABLblqZhCaycjAbjxv8TK2odMakRq2hNQjGq9YFOvxPY3EugQqilOZAwQ3DQsm6208TeXQsrE*', 'Used to get access token')


