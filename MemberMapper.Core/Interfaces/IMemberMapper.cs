using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MemberMapper.Core.Interfaces
{
  public interface IMemberMapper
  {
    IMappingStrategy MappingStrategy { get; set; }

    TDestination Map<TDestination>(object source) where TDestination : new();

    TSource Map<TSource>(TSource source) where TSource : new();

    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    IProposedMap CreateMap(Type source, Type destination, Action<PropertyInfo, IMappingOption> options = null);

    void RegisterMap(IMemberMap map);

  }
}
