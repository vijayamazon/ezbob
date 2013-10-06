root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.CustomerFullModel extends Backbone.Model
    url: -> "#{window.gRootPath}Underwriter/FullCustomer/Index/?id=#{@get("customerId")}&history=#{@get("history")}"