using System;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public interface IDatabaseFunctionFactory<in T>
	{
		IDatabaseFunction Create(T enumItem);

		IDatabaseFunction GetById( Guid id );
	}
	
}