﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using Ninject;
using System.Linq.Expressions;

namespace ThisMember.Core
{

  public class MapNotFoundException : Exception
  {

    public Type SourceType { get; set; }

    public Type DestinationType { get; set; }
    public MapNotFoundException(Type source, Type destination)
      : base(string.Format("No mapping between {0} and {1} exists", source, destination))
    {
      this.SourceType = source;
      this.DestinationType = destination;
    }
  }

  public class MemberMapper : IMemberMapper
  {

    private readonly MapperOptions options;

    public MemberMapper(MapperOptions options = null, IMappingStrategy strategy = null, IMapGenerator generator = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy(this);

      this.MappingStrategy.MapGenerator = generator ?? new CompiledMapGenerator();

      this.options = options ?? MapperOptions.Default;
    }

    private Dictionary<TypePair, MemberMap> maps = new Dictionary<TypePair, MemberMap>();

    public TDestination Map<TDestination>(object source) where TDestination : new()
    {
      var pair = new TypePair(source.GetType(), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMap(pair).FinalizeMap();
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

    public ProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      var proposedMap = this.MappingStrategy.CreateMap<TSource, TDestination>(options, customMapping);

      return proposedMap;
    }

    public ProposedMap CreateMap(Type source, Type destination, MappingOptions options = null)
    {

      var pair = new TypePair(source, destination);

      var proposedMap = this.MappingStrategy.CreateMap(pair, options);

      return proposedMap;

    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMap(pair).FinalizeMap();
      }
      if (options.BeforeMapping != null) options.BeforeMapping();

      var result = ((Func<TSource, TDestination, TDestination>)map.MappingFunction)(source, destination);

      if (options.AfterMapping != null) options.AfterMapping();

      return result;

    }


    public void RegisterMap(MemberMap map)
    {
      this.maps[new TypePair(map.SourceType, map.DestinationType)] = map;
    }

    public TSource Map<TSource>(TSource source) where TSource : new()
    {
      var destination = new TSource();

      return Map(source, destination);

    }

    public IMappingStrategy MappingStrategy { get; set; }

    public MemberMap CreateAndFinalizeMap(Type source, Type destination, MappingOptions options = null)
    {
      return CreateMap(source, destination, options).FinalizeMap();
    }

    public MemberMap<TSource, TDestination> CreateAndFinalizeMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      return CreateMap<TSource, TDestination>(options, customMapping).FinalizeMap().ToGeneric<TSource, TDestination>();
    }

    public bool HasMap<TSource, TDestination>()
    {
      return this.maps.ContainsKey(new TypePair(typeof(TSource), typeof(TDestination)));
    }

    public bool HasMap(Type source, Type destination)
    {
      return this.maps.ContainsKey(new TypePair(source, destination));
    }

    public MemberMap GetMap<TSource, TDestination>()
    {
      MemberMap map;

      if (!this.maps.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out map))
      {
        throw new MapNotFoundException(typeof(TSource), typeof(TDestination));
      }
      return map;

    }

    public MemberMap GetMap(Type source, Type destination)
    {
      MemberMap map;

      if (!this.maps.TryGetValue(new TypePair(source, destination), out map))
      {
        throw new MapNotFoundException(source, destination);
      }
      return map;
    }

    public bool TryGetMap<TSource, TDestination>(out MemberMap map)
    {
      if (!this.maps.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out map))
      {
        return false;
      }
      return true;
    }

    public bool TryGetMap(Type source, Type destination, out MemberMap map)
    {
      if (!this.maps.TryGetValue(new TypePair(source, destination), out map))
      {
        return false;
      }
      return true;
    }
  }
}
