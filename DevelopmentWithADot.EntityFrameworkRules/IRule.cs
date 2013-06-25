using System;
using System.Data.Entity;

namespace DevelopmentWithADot.EntityFrameworkRules
{
	public interface IRule<T>
	{
		Boolean CanSave(T entity, DbContext ctx);
		Boolean CanUpdate(T entity, DbContext ctx);
		Boolean CanDelete(T entity, DbContext ctx);
		String Name
		{
			get;
		}
	}
}
