_.extend(Backbone.Collection.prototype, {
    where: function (attrs, first) {
        if (_.isEmpty(attrs)) return first ? void 0 : [];
        return this[first ? 'find' : 'filter'](function (model) {
            for (var key in attrs) {
                if (attrs[key] !== model.get(key)) return false;
            }
            return true;
        });
    }
});