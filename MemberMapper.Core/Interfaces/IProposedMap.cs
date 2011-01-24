using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedMap
  {
    IMemberMap FinalizeMap();

    IProposedTypeMapping ProposedTypeMapping { get; }
  }

  public interface IProposedMap<TSource, TDestination> : IProposedMap
  {
    IProposedMap<TSource, TDestination> AddExpression(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination);
  }
}
