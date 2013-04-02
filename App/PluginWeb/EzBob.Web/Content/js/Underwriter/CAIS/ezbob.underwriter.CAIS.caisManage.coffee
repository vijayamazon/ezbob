root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.CAIS = EzBob.Underwriter.CAIS or {}

class EzBob.Underwriter.CAIS.CaisManageView extends Backbone.Marionette.ItemView
    template: _.template(if $("#cais-template").length>0 then $("#cais-template").html() else "")

    events:
        "click .generate":"generateClicked"
        "click .OpenFiles":"OpenFilesClicked"
        "change [name='files']": "filesFieldChanged"

    ui:
        "form":"form"

    generateClicked: ->
        $.post(gRootPath + 'Underwriter/CAIS/Generate')
        .done (response)->
            if response.error != undefined
                EzBob.ShowMessage "Something went wrong", "Error occured"
                return
            EzBob.ShowMessage "Generating current CAIS reports was starting. Please wait few minute", "Successful"
            
    OpenFilesClicked: ->
        $('[name="files"]').click()

    filesFieldChanged: (e)->
        $el = $(e.currentTarget)[0]
        files = if $el.files then $el.files else $el.val()
        fileNames = [];
        _.each files, (val)=>
            fileNames.push val.name #val.replace(/.*\\/g,'')

        okAction = => 
            BlockUi "on"
            @ui.form.ajaxSubmit
                cache:false
                success: (response)=>
                    if response.error != undefined
                         EzBob.ShowMessage response.error, "Error occured"
                         return
                    EzBob.ShowMessage "File{0} successfully downloaded".f(if fileNames.length == 1 then "" else "s"), "Successful"
                error: (response)=>
                    error = response.responseText
                    if response.responseText.indexOf("<title>") > 0
                        #parse error
                        x1 = response.responseText.indexOf("<title>")+7;
                        x2 = response.responseText.indexOf("</title>") - x1;
                        error = response.responseText.substr(x1, x2);
                    EzBob.ShowMessage error, "Error"
                complete: =>
                    BlockUi "off"

        EzBob.ShowMessage("Do you want to sent <b>{0}</b>?".f(fileNames), "Warning",  okAction, "OK", null, "Cancel")