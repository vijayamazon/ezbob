using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public interface IDatabaseFunctionFactory<in T>
	{
		IDatabaseFunction Create(T enumItem);

		IDatabaseFunction GetById( Guid id );
	    IEnumerable<IDatabaseFunction> GetAll();
	}	
}