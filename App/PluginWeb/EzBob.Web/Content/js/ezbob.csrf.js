$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    var verificationToken = $("input[name='__RequestVerificationToken']").val();
    if (verificationToken) {
        jqXHR.setRequestHeader("X-Request-Verification-Token", verificationToken);
    }
});