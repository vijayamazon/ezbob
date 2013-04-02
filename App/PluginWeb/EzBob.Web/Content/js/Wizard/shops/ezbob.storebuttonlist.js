(function() {

  EzBob.StoreButtonWithListView = EzBob.StoreButtonView.extend({
    initialize: function() {
      this.listView.model.on("change", this.listChanged, this);
      this.listView.model.on("reset", this.listChanged, this);
      return EzBob.StoreButtonView.prototype.initialize.apply(this, arguments_);
    },
    listChanged: function() {
      if (this.listView.model.length > 0) {
        return this.trigger("ready", this.name);
      }
    },
    render: function() {
      EzBob.StoreButtonView.prototype.render.apply(this);
      this.$el.append(this.listView.render().$el);
      return this;
    },
    update: function() {
      return this.listView.update();
    }
  });

}).call(this);
