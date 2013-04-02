$('<div/>').on 'ajaxComplete', (e, xhr) ->
        try
            return unless xhr.responseText? or xhr.responseText == ""
            data = JSON.parse xhr.responseText
            return unless data.error?
            console.log data.error
            alertify.error data.error