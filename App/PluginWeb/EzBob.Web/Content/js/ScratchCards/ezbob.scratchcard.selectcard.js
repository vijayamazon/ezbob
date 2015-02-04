var EzBob = EzBob || {};
EzBob.ScratchCards = EzBob.ScratchCards || {};

EzBob.ScratchCards.Select = function(lotteryCode, scratchArgs) {
	if (!lotteryCode)
		return null;

	switch (lotteryCode) {
	case 'ny2015':
		return new EzBob.ScratchCards.Ny2015(scratchArgs);

	case 'val2015':
		return new EzBob.ScratchCards.Valentine2015(scratchArgs);

	default:
		return null;
	} // switch
}; // EzBob.ScratchCards.Select
