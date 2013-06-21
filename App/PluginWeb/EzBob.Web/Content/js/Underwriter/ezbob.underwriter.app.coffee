$('<div/>').on 'ajaxComplete', (e, xhr) ->
        try
            return unless xhr.responseText? or xhr.responseText == "" or typeof xhr.responseText is 'string'
            data = JSON.parse xhr.responseText
            return unless data.error?
            alertify.error data.error