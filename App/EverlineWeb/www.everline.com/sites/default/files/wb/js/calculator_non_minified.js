var g_rounding_decimal_places = 2;
var g_calc_utils = new CalculatorUtils();

/* 
 *	Calculator Object 
 */
function Calculator(calculator_id, interest_rate, min_period, min_amount, initial_period, initial_amount, animation_delay) {

	this.calculator_id = calculator_id;
	this.loan_period_months = 1;
	this.loan_amount = 1000;
	this.payments_array;
	this.total_repayment;
	this.animation_delay = animation_delay;
	this.calculator_table = jQuery('#' + calculator_id + ' table');
	
	// cache the slider objects
	this.amount_slider = jQuery('#' + calculator_id + ' #amount');
	this.period_slider = jQuery('#' + calculator_id + ' #period');
		
	var self = this;
	
	// config parameters
	this.interest_rate = interest_rate;	
	this.min_period = min_period;
	this.min_amount = min_amount;	
	this.initial_period = initial_period;
	this.initial_amount = initial_amount;
	
	// save the ids of all elements we'll be using
	this.loan_amount_text = jQuery('#' + calculator_id + ' #loan-amount-text');
	this.loan_period_text = jQuery('#' + calculator_id + ' #loan-period-text');
	this.loan_repayment_text = jQuery('#' + calculator_id + ' #loan-repayment-text');
	
	// hook up slider events
	this.amount_slider.bind('slider:changed', function (event, data) {
		self.amountChanged(data.value);
	});
	this.period_slider.bind('slider:changed', function (event, data) {
		self.periodChanged(data.value);
	});	
	
	// event handlers
	this.amountChanged = function(newValue) {
		this.loan_amount = newValue;
		this.loan_amount_text.html('£' + g_calc_utils.formatForPrint(newValue));
		this.updateRepaymentValue();
	}
	this.periodChanged = function(newValue) {
		this.loan_period_months = newValue;
		this.loan_period_text.html(newValue + (newValue > 1 ? ' months' : ' month'));
		this.updateRepaymentValue();
	}
	
	// set slider values programmatically
	this.setPeriod = function(newValue) {
		this.period_slider.simpleSlider("setValue", newValue, true);
	}
	this.setAmount = function(newValue) {
		this.amount_slider.simpleSlider("setValue", newValue, true);
	}	
	
	// recalculation and re-rendering routines
	this.calcRepaymentsTable = function() {
		this.payments_array = new Array();
		this.total_repayment = 0;
		this.total_interest = 0;
		
		// remove all table rows if table exists
		if (this.calculator_table)
			this.calculator_table.find("tr:gt(0)").remove();		
		
		// calculate the repayments table
		for (var i = 0; i < this.loan_period_months; i++) {
			var monthly_repayment = new MonthlyPayment(i, this.loan_period_months, this.loan_amount, this.interest_rate)
			this.payments_array.push(monthly_repayment);
			this.total_repayment += monthly_repayment.getTotal();
			this.total_interest += monthly_repayment.getInterest();
			
			if (this.calculator_table) {
				jQuery('tr:last', this.calculator_table).after('<tr><td>'+(i+1)+'</td><td>£'+g_calc_utils.formatForPrint(monthly_repayment.getPrincipal())+'</td><td>£'+g_calc_utils.formatForPrint(monthly_repayment.getInterest())+'</td><td>£'+g_calc_utils.formatForPrint(monthly_repayment.getTotal())+'</td></tr>');
			}
		}
		
		// add rows to the table if table exists
		if (this.calculator_table) {
			jQuery('#' + this.calculator_id + ' #total_amount').text(g_calc_utils.formatForPrint(this.loan_amount));
			jQuery('#' + this.calculator_id + ' #total_interest').text(g_calc_utils.formatForPrint(this.total_interest));
			jQuery('#' + this.calculator_id + ' #total_repayment').text(g_calc_utils.formatForPrint(this.total_repayment));
		}
		
		if (this.setCookies)
			this.setCookies(this.loan_amount, this.loan_period_months);
		
		//console.log('Repayments: ', this.payments_array);
	}	
	
	this.updateRepaymentValue = function() {
		this.calcRepaymentsTable();
		this.loan_repayment_text.html('£' + g_calc_utils.formatForPrint(this.total_repayment));
	}
	
	// Initialization
	this.initialize = function() {
		self.setAmount(self.initial_amount);
		self.setPeriod(self.initial_period);		
	}
	
	this.amountChanged(initial_amount);
	this.periodChanged(initial_period);

	jQuery(document).ready(function() {
		window.setTimeout(function() { self.initialize(); }, self.animation_delay);
	});
		
	this.setCookies = function(amount, period) {
		setCookie("loan_amount", amount, 1, "/", "ezbob.com");
		setCookie("loan_period", period, 1, "/", "ezbob.com");
	}
}

/* 
 *	Monthly Payment Object
 */
 function MonthlyPayment(month_id, loan_period, loan_amount, interest_rate_percent) {
	this.month_id = month_id;
	this.loan_amount = loan_amount;
	this.loan_period = loan_period;
	this.principal = loan_amount / loan_period;
	this.remaining_balance = loan_amount * (loan_period - month_id) / loan_period;
	this.interest = this.remaining_balance * interest_rate_percent / 100;
	this.total_monthly = this.principal + this.interest;
	
	this.getPrincipal = function() {
		return this.principal;
	}
	
	this.getInterest = function() {
		return this.interest;
	}
	
	this.getTotal = function() {
		return this.total_monthly;
	}
 }
 
/* 
*	CalculatorUtils Scope 
*/
function CalculatorUtils() {
	this.roundDecimalPlaces = function(value, num_decimal_places) {
		return value.toFixed(num_decimal_places);
	}
	
	this.numberWithCommas = function(x) {
		return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
	}
	
	this.formatForPrint = function(x) {
		return this.numberWithCommas(this.roundDecimalPlaces(x, 0));
	}
}
 

/* 
*	Calculator Cookie Handling
*/ 
function setCookie(name, value, expires, path, domain, secure) {
	cookieStr = name + "=" + escape(value) + "; ";
	
	if(expires){
		expires = setExpiration(expires);
		cookieStr += "expires=" + expires + "; ";
	}
	if(path){
		cookieStr += "path=" + path + "; ";
	}
	if(domain){
		cookieStr += "domain=" + domain + "; ";
	}
	if(secure){
		cookieStr += "secure; ";
	}
	
	document.cookie = cookieStr;
}

function setExpiration(cookieLife){
    var today = new Date();
    var expr = new Date(today.getTime() + cookieLife * 24 * 60 * 60 * 1000);
    return  expr.toGMTString();
}