var EzBob = EzBob || {};

EzBob.setLegalDocsEvents = function () {
    $('#LegalDocsDDL').on('change', function () {
        EzBob.setLegalDocView();
    });

};

EzBob.setLegalDocView = function (ev) {
    var selectedValue = ev.selectedOptions[0].value;
    $.ajax({
        type: "GET",
        url: '/LegalDocs/GetLegalDoc/' + selectedValue,
        async: false,
        dataType: "json",
        success: function (data) {
            tinyMCE.activeEditor.setContent(data);
            //$('body textarea').val(data);
        }
    });
}

EzBob.SaveDoc = function () {
    var obj = {
        id: $('#LegalDocsDDL').find('option:selected').val(),
        name: $('#LegalDocsDDL').find('option:selected').text(),
        text: tinyMCE.activeEditor.getContent()
    };
    $.ajax({
        url: '/LegalDocs/SaveLegalDoc',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(obj),
        success: function (result) {
            alert('the request was successfully sent to the server');
        }
    });
};


