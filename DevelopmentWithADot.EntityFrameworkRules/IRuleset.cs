using System;
using System.Collections.Generic;

namespace DevelopmentWithADot.EntityFrameworkRules
{
	public interface IRuleset : IDisposable
	{
		void AddRule<T>(IRule<T> rule);
		IEnumerable<IRule<T>> GetRules<T>();
	}
}
