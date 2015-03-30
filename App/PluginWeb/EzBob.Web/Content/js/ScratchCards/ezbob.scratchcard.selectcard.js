var EzBob = EzBob || {};
EzBob.ScratchCards = EzBob.ScratchCards || {};

EzBob.ScratchCards.Base = EzBob.View.extend({
	show: function() {
		$('#promotion-pages').show();
	}, // show

	hide: function() {
		$('#promotion-pages').hide();
	}, // hide
}); // EzBob.ScratchCards.Base

EzBob.ScratchCards.Select = function(lotteryCode, scratchArgs) {
	if (!lotteryCode)
		return null;

	switch (lotteryCode) {
	case 'ny2015':
		return new EzBob.ScratchCards.Ny2015(scratchArgs);

	case 'val2015':
		return new EzBob.ScratchCards.Valentine2015(scratchArgs);

	case 'easter2015':
		return new EzBob.ScratchCards.Easter2015(scratchArgs);

	default:
		return null;
	} // switch
}; // EzBob.ScratchCards.Select
