using System;
using System.Collections.Generic;
using System.Data.Objects;

namespace DevelopmentWithADot.EntityFrameworkRules
{
	sealed class Ruleset : IRuleset
	{
		private readonly IDictionary<Type, HashSet<Object>> rules = new Dictionary<Type, HashSet<Object>>();
		private ObjectContext octx = null;

		internal Ruleset(ObjectContext octx)
		{
			this.octx = octx;
		}

		public void AddRule<T>(IRule<T> rule)
		{
			if (this.rules.ContainsKey(typeof(T)) == false)
			{
				this.rules[typeof(T)] = new HashSet<Object>();
			}

			this.rules[typeof(T)].Add(rule);
		}

		public IEnumerable<IRule<T>> GetRules<T>()
		{
			if (this.rules.ContainsKey(typeof(T)) == true)
			{
				foreach (IRule<T> rule in this.rules[typeof(T)])
				{
					yield return (rule);
				}
			}
		}

		public void Dispose()
		{
			this.octx.SavingChanges -= RulesExtensions.OnSaving;
			RulesExtensions.rulesets.Remove(this.octx);
			this.octx = null;

			this.rules.Clear();
		}
	}
}
