using ALB.Domain.Entities;

using Ardalis.Specification;

using Microsoft.EntityFrameworkCore;

namespace ALB.Domain.Specifications;

public class ChildrenByFirstOrLastnameSpec: Specification<Child>
{
    /// <summary>
    /// Search for children by first or last name by cursor paging
    /// </summary>
    /// <param name="cursor">Guid which may be null</param>
    /// <param name="limit">Top number of searches</param>
    /// <param name="name">First- or Lastname</param>
    public ChildrenByFirstOrLastnameSpec(Guid? cursor, int limit, string name)
    {
        if (cursor is not null)
        {
            Query.OrderByDescending(x => x.Id)
                .Where(c => c.Id <= cursor)
                .Where(x => 
                    EF.Functions.Like(x.FirstName, "%" + name + "%") || 
                    EF.Functions.Like(x.LastName, "%" + name + "%"))
                .Take(limit + 1);
        }
        else
        {
            Query
                .OrderByDescending(x => x.Id)
                .Where(x => 
                    EF.Functions.Like(x.FirstName, "%" + name + "%") || 
                    EF.Functions.Like(x.LastName, "%" + name + "%"))
                .Take(limit + 1);
        }
    }
}