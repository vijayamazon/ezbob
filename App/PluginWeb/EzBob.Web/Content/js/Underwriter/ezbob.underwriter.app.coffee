$('<div/>').on 'ajaxComplete', (e, xhr) ->
        try
            if xhr.status is 423
                console.log "request is not authorized by server. refreshing page"
                document.location.href = document.location.href
                return
            return unless xhr.responseText? or xhr.responseText == "" or typeof xhr.responseText is 'string'
            data = JSON.parse xhr.responseText
            return unless data.error?
            alertify.error data.error