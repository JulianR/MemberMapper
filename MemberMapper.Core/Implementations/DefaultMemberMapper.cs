using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using Ninject;

namespace MemberMapper.Core.Implementations
{
  public class DefaultMemberMapper : IMemberMapper
  {
    public DefaultMemberMapper(IMappingStrategy strategy = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy();
      this.MappingStrategy.Mapper = this;
    }

    private Dictionary<TypePair, IMemberMap> maps = new Dictionary<TypePair, IMemberMap>();

    public TDestination Map<TDestination>(object source)
    {
      throw new NotImplementedException();
    }

    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : new()
    {
      var destination = new TDestination();

      return Map(source, destination);
    }

    public IProposedMap CreateMap(Type source, Type destination, Action<System.Reflection.PropertyInfo, IMappingOption> options = null)
    {
      IMemberMap map;

      var pair = new TypePair(source, destination);

      if (!this.maps.TryGetValue(pair, out map))
      {
        var proposedMap = this.MappingStrategy.CreateMap(pair, options);

        return proposedMap;
      }

      return map;

    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      IMemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateAndFinalizeMap(pair);
      }

      return ((Func<TSource, TDestination, TDestination>)map.MappingFunction)(source, destination);
    }


    public void RegisterMap(IMemberMap map)
    {
      this.maps.Add(new TypePair(map.SourceType, map.DestinationType), map);
    }

    public TSource Map<TSource>(TSource source) where TSource : new()
    {
      var destination = new TSource();

      return Map(source, destination);

    }

    public IMappingStrategy MappingStrategy { get; set; }

  }
}
