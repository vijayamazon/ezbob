root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

$('body').on 'click', 'button[name="newCreditLineLnk"]', (e) ->
    $('#newCreditLineButtonId').click()
