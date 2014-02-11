﻿var EzBob = EzBob || {};

EzBob.StrengthPassword = Backbone.Model.extend({
	defaults: {
		message: "password strength: ",
		status: "none",
		color: "white"
	},

	estimatePassword: function(password) {

		if (!password) {
			this.set({ color: "black", status: "none" });
			return;
		}

		var minPassLength = EzBob.Config.PasswordPolicyType == "hard" ? 7 : 6;

		if (password.length < minPassLength) {
			this.set({ color: "red", status: "The password should be {0} characters or more".f(minPassLength), showTitle: false });
			return;
		}

		var score = this.testPassword(password);

		if (score < 16) {
			this.set({ color: "red", status: "weak", showTitle: true });
		}
		else if (score > 15 && score < 25) {
			this.set({ color: "red", status: "weak", showTitle: true });
		}
		else if (score > 24 && score < 35) {
			this.set({ color: "blue", status: "normal", showTitle: true });
		}
		else if (score > 34 && score < 45) {
			this.set({ color: "green", status: "strong", showTitle: true });
		}
		else {
			this.set({ color: "green", status: "strong", showTitle: true });
		}
	},
	testPassword: function(passwd) {
		var intScore = 0;
		var strVerdict = "weak";
		var strLog = "";

		var passLength = passwd.length;

		if (passLength < 5)                         // length 4 or less
		{
			intScore = (intScore + 3);
			strLog = strLog + "3 points for length (" + passLength + ")\n";
		}
		else if (passLength > 4 && passLength < 8) // length between 5 and 7
		{
			intScore = (intScore + 6);
			strLog = strLog + "6 points for length (" + passLength + ")\n";
		}
		else if (passLength > 7 && passLength < 16)// length between 8 and 15
		{
			intScore = (intScore + 12);
			strLog = strLog + "12 points for length (" + passLength + ")\n";
		}
		else if (passLength > 15)                    // length 16 or more
		{
			intScore = (intScore + 18);
			strLog = strLog + "18 point for length (" + passLength + ")\n";
		}


		// LETTERS (Not exactly implemented as dictacted above because of my limited understanding of Regex)
		if (passwd.match(/[a-z]/))                              // [verified] at least one lower case letter
		{
			intScore = (intScore + 1);
			strLog = strLog + "1 point for at least one lower case char\n";
		}

		if (passwd.match(/[A-Z]/))                              // [verified] at least one upper case letter
		{
			intScore = (intScore + 5);
			strLog = strLog + "5 points for at least one upper case char\n";
		}

		// NUMBERS
		if (passwd.match(/\d+/))                                 // [verified] at least one number
		{
			intScore = (intScore + 5);
			strLog = strLog + "5 points for at least one number\n";
		}

		if (passwd.match(/(.*[0-9].*[0-9].*[0-9])/))             // [verified] at least three numbers
		{
			intScore = (intScore + 5);
			strLog = strLog + "5 points for at least three numbers\n";
		}


		// SPECIAL CHAR
		if (passwd.match(/.[!,@,#,$,%,^,&,*,?,_,~]/))            // [verified] at least one special character
		{
			intScore = (intScore + 5);
			strLog = strLog + "5 points for at least one special char\n";
		}

		// [verified] at least two special characters
		if (passwd.match(/(.*[!,@,#,$,%,^,&,*,?,_,~].*[!,@,#,$,%,^,&,*,?,_,~])/)) {
			intScore = (intScore + 5);
			strLog = strLog + "5 points for at least two special chars\n";
		}


		// COMBOS
		if (passwd.match(/([a-z].*[A-Z])|([A-Z].*[a-z])/))        // [verified] both upper and lower case
		{
			intScore = (intScore + 2);
			strLog = strLog + "2 combo points for upper and lower letters\n";
		}

		if (passwd.match(/([a-zA-Z])/) && passwd.match(/([0-9])/)) // [verified] both letters and numbers
		{
			intScore = (intScore + 2);
			strLog = strLog + "2 combo points for letters and numbers\n";
		}

		// [verified] letters, numbers, and special characters
		if (passwd.match(/([a-zA-Z0-9].*[!,@,#,$,%,^,&,*,?,_,~])|([!,@,#,$,%,^,&,*,?,_,~].*[a-zA-Z0-9])/)) {
			intScore = (intScore + 2);
			strLog = strLog + "2 combo points for letters, numbers and special chars\n";
		}

		return intScore;
	}
});

EzBob.StrengthPasswordView = Backbone.View.extend({
	events: {
		"keyup input:password": "show",
		"input input:password": "show"
	},

	initialize: function() {
		this.model.on('change', this.render, this);
		this.passwordSelector = this.options.passwordSelector || 'input#signupPass1';
	},

	show: function() {
		var password = this.$el.find(this.passwordSelector).val().toString();
		this.model.estimatePassword(password);
	},

	render: function() {
		var status = this.model.get('status');
		var color = this.model.get('color');

		if (status == "none") {
			this.$el.find('#message').hide();
		}
		else {
			this.$el.find('#message').css({ color: color });
			this.$el.find('#message').text((this.model.get('showTitle') ? this.model.get('message') : '') + status);
			this.$el.find('#message').show();
		}

		return this;
	}
});

