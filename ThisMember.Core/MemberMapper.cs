using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Interfaces;
using Ninject;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  public class MemberMapper : IMemberMapper
  {

    private readonly MapperOptions options;

    public MemberMapper(MapperOptions options = null, IMappingStrategy strategy = null, IMapGenerator generator = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy();

      this.MappingStrategy.MapGenerator = generator ?? new CompiledMapGenerator();

      this.options = options ?? MapperOptions.Default;

      this.MappingStrategy.MemberMapCreated += this.RegisterMap;

    }

    private Dictionary<TypePair, IMemberMap> maps = new Dictionary<TypePair, IMemberMap>();

    public TDestination Map<TDestination>(object source) where TDestination : new()
    {
      var pair = new TypePair(source.GetType(), typeof(TDestination));

      IMemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateAndFinalizeMap(pair);
      }

      var destination = new TDestination();

      if (options.BeforeMapping != null) options.BeforeMapping();

      var result = (TDestination)map.MappingFunction.DynamicInvoke(source, destination);

      if (options.AfterMapping != null) options.AfterMapping();

      return result;
    }

    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : new()
    {
      TDestination destination = default(TDestination);

      if (source != null)
      {
        destination = new TDestination();
      }

      return Map(source, destination);
    }

    public IProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      IMemberMap map;

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      if (!this.maps.TryGetValue(pair, out map))
      {
        var proposedMap = this.MappingStrategy.CreateMap<TSource, TDestination>(options,customMapping);

        return proposedMap;
      }

      return map.ToGeneric<TSource, TDestination>();
    }

    public IProposedMap CreateMap(Type source, Type destination, MappingOptions options = null)
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
      if (options.BeforeMapping != null) options.BeforeMapping();

      var result = ((Func<TSource, TDestination, TDestination>)map.MappingFunction)(source, destination);

      if (options.AfterMapping != null) options.AfterMapping();

      return result;

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
