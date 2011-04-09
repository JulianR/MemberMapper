using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace ThisMember.Core
{
  public class ProposedMap : IProposedMap
  {

    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public CustomMapping CustomMapping { get; set; }

    public IMapGenerator MapGenerator { get; set; }

    public ProposedMap()
    {
    }

    public event Action<ProposedMap, IMemberMap> MemberMapCreated;


    public IMemberMap FinalizeMap()
    {
      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = this.MapGenerator.GenerateMappingFunction(this);

      if (MemberMapCreated != null)
      {
        MemberMapCreated(this, map);
      }

      return map;
    }

    public IProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap, IProposedMap<TSource, TDestination>
  {
    public IProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      throw new NotImplementedException();
    }
  }
}
