using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Reflection;

namespace DevelopmentWithADot.EntityFrameworkRules
{
	public static class RulesExtensions
	{
		private static readonly MethodInfo getRulesMethod = typeof(IRuleset).GetMethod("GetRules");
		internal static readonly IDictionary<ObjectContext, Tuple<IRuleset, DbContext>> rulesets = new Dictionary<ObjectContext, Tuple<IRuleset, DbContext>>();

		private static Type GetRealType(Object entity)
		{
			return (entity.GetType().Assembly.IsDynamic == true ? entity.GetType().BaseType : entity.GetType());
		}

		internal static void OnSaving(Object sender, EventArgs e)
		{
			ObjectContext octx = sender as ObjectContext;
			IRuleset ruleset = rulesets[octx].Item1;
			DbContext ctx = rulesets[octx].Item2;

			foreach (ObjectStateEntry entry in octx.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
			{
				Object entity = entry.Entity;
				Type realType = GetRealType(entity);

				foreach (dynamic rule in (getRulesMethod.MakeGenericMethod(realType).Invoke(ruleset, null) as IEnumerable))
				{
					if (rule.CanSave(entity, ctx) == false)
					{
						throw (new Exception(String.Format("Cannot save entity {0} due to rule {1}", entity, rule.Name)));
					}
				}
			}

			foreach (ObjectStateEntry entry in octx.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted))
			{
				Object entity = entry.Entity;
				Type realType = GetRealType(entity);

				foreach (dynamic rule in (getRulesMethod.MakeGenericMethod(realType).Invoke(ruleset, null) as IEnumerable))
				{
					if (rule.CanDelete(entity, ctx) == false)
					{
						throw (new Exception(String.Format("Cannot delete entity {0} due to rule {1}", entity, rule.Name)));
					}
				}
			}

			foreach (ObjectStateEntry entry in octx.ObjectStateManager.GetObjectStateEntries(EntityState.Modified))
			{
				Object entity = entry.Entity;
				Type realType = GetRealType(entity);

				foreach (dynamic rule in (getRulesMethod.MakeGenericMethod(realType).Invoke(ruleset, null) as IEnumerable))
				{
					if (rule.CanUpdate(entity, ctx) == false)
					{
						throw (new Exception(String.Format("Cannot update entity {0} due to rule {1}", entity, rule.Name)));
					}
				}
			}
		}

		public static IRuleset CreateRuleset(this DbContext context)
		{
			Tuple<IRuleset, DbContext> ruleset = null;
			ObjectContext octx = (context as IObjectContextAdapter).ObjectContext;

			if (rulesets.TryGetValue(octx, out ruleset) == false)
			{
				ruleset = rulesets[octx] = new Tuple<IRuleset, DbContext>(new Ruleset(octx), context);
				
				octx.SavingChanges += OnSaving;
			}

			return (ruleset.Item1);
		}
	}
}
