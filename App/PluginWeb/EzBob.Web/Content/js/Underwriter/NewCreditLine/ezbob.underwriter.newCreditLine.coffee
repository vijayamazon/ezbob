root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

$('body').on 'click', 'a[name="newCreditLineLnk"]', (e) ->
    $('#newCreditLineButtonId').click()
