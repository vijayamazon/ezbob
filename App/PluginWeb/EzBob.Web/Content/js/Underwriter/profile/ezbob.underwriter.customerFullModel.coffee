root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.CustomerFullModel extends Backbone.Model
    idAttribute: "Id"
    urlRoot: "#{window.gRootPath}Underwriter/FullCustomer/Index"
