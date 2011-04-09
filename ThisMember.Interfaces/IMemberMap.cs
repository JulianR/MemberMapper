using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Interfaces
{
  public interface IMemberMap : IProposedMap
  {
    Type SourceType { get; set; }
    Type DestinationType { get; set; }

    Delegate MappingFunction { get; set; }

    IMemberMap<TSource, TDestination> ToGeneric<TSource, TDestination>();

  }

  public interface IMemberMap<TSource, TDestination> : IProposedMap<TSource, TDestination>
  {
  }
}
