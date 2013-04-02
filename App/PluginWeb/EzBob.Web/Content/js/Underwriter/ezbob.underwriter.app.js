(function() {

  $('<div/>').on('ajaxComplete', function(e, xhr) {
    var data;
    try {
      if (!((xhr.responseText != null) || xhr.responseText === "")) {
        return;
      }
      data = JSON.parse(xhr.responseText);
      if (data.error == null) {
        return;
      }
      console.log(data.error);
      return alertify.error(data.error);
    } catch (_error) {}
  });

}).call(this);
