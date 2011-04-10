using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core;

namespace ThisMember.Interfaces
{
  public interface IProposedMap
  {
    IMemberMap FinalizeMap();

    IProposedTypeMapping ProposedTypeMapping { get; set; }

    Type SourceType { get; }
    Type DestinationType { get; }

    IMapGenerator MapGenerator { get; set; }

    CustomMapping CustomMapping { get; set; }

  }

  public interface IProposedMap<TSource, TDestination> : IProposedMap
  {
    IProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn;

  }
}
