var EzBob = EzBob || {};

EzBob.validateAdminLoginForm = function(el) {
	var e = el || $(".simple-login");
	return e.validate({
		rules: {
			UserName: { required: true },
			Password: { required: true }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlight
		//highlight: EzBob.Validation.highlight
	});
};
