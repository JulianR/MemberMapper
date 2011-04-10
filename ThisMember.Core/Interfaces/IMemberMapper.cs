using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{
  public interface IMemberMapper
  {
    IMappingStrategy MappingStrategy { get; set; }

    TDestination Map<TDestination>(object source) where TDestination : new();

    TSource Map<TSource>(TSource source) where TSource : new();

    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    ProposedMap CreateMap(Type source, Type destination, MappingOptions options = null);

    ProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    MemberMap CreateAndFinalizeMap(Type source, Type destination, MappingOptions options = null);

    MemberMap<TSource, TDestination> CreateAndFinalizeMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);


    void RegisterMap(MemberMap map);

  }
}
