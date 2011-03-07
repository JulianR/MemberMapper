using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;

namespace MemberMapper.Core.Implementations
{
  public class MemberMap : IMemberMap
  {
    public IMemberMap FinalizeMap()
    {
      return this;
    }

    public IProposedTypeMapping ProposedTypeMapping
    {
      get { throw new NotImplementedException(); }
    }

    #region IMemberMap Members

    public Type SourceType
    {
      get;
      set;
    }

    public Type DestinationType
    {
      get;
      set;
    }

    public Delegate MappingFunction
    {
      get;
      set;
    }

    #endregion

    public IMemberMap<TSource, TDestination> ToGeneric<TSource, TDestination>()
    {
      var map = new MemberMap<TSource, TDestination>();

      map.DestinationType = this.DestinationType;
      map.SourceType = this.SourceType;
      map.MappingFunction = this.MappingFunction;

      return map;
    }

    #region IProposedMap Members


    public IMapGenerator MapGenerator
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    #endregion
  }

  public class MemberMap<TSource, TDestination> : MemberMap, IMemberMap<TSource, TDestination>
  {

    public IProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(System.Linq.Expressions.Expression<Func<TSource, TSourceReturn>> source, System.Linq.Expressions.Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      throw new NotImplementedException();
    }
  }
}
