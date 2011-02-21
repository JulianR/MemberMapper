using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Reflection;
using System.Collections;

namespace MemberMapper.Core.Implementations
{
  public class DefaultMappingStrategy : IMappingStrategy
  {

    public IMemberMapper Mapper { get; set; }

    public DefaultMappingStrategy(IMemberMapper mapper)
    {
      this.Mapper = mapper;
    }

    public DefaultMappingStrategy()
    {
    }

    private Dictionary<TypePair, ProposedTypeMapping> mappingCache = new Dictionary<TypePair, ProposedTypeMapping>();

    private Type GetTypeInsideEnumerable(PropertyInfo prop)
    {
      var getEnumeratorMethod = prop.PropertyType.GetMethod("GetEnumerator", Type.EmptyTypes);

      if (getEnumeratorMethod == null) return null;

      if (getEnumeratorMethod.ReturnType.IsGenericType)
      {
        return getEnumeratorMethod.ReturnType.GetGenericArguments().First();
      }

      return typeof(object);

    }

    private ProposedTypeMapping GetTypeMapping(TypePair pair, Action<PropertyInfo, IMappingOption> options = null)
    {


      var typeMapping = new ProposedTypeMapping();

      typeMapping.SourceMember = null;
      typeMapping.DestinationMember = null;

      var destinationProperties = (from p in pair.DestinationType.GetProperties()
                                   where p.CanWrite
                                   select p).ToDictionary(k => k.Name);

      var sourceProperties = (from p in pair.SourceType.GetProperties()
                              where p.CanRead
                              select p);

      foreach (var property in sourceProperties)
      {
        PropertyInfo match;

        if (destinationProperties.TryGetValue(property.Name, out match)
          && match.PropertyType.IsAssignableFrom(property.PropertyType))
        {

          if (options != null)
          {
            var option = new MappingOption();

            options(match, option);

            switch (option.State)
            {
              case MappingOptionState.Ignored:
                continue;
            }

          }

          typeMapping.ProposedMappings.Add
          (
            new ProposedMemberMapping
            {
              From = property,
              To = match
            }
          );
        }
        else if (match != null)
        {

          if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
            && typeof(IEnumerable).IsAssignableFrom(match.PropertyType))
          {

            var typeOfSourceEnumerable = GetTypeInsideEnumerable(property);
            var typeOfDestinationEnumerable = GetTypeInsideEnumerable(match);

            if (typeOfDestinationEnumerable == typeOfSourceEnumerable)
            {
              typeMapping.ProposedMappings.Add
              (
                new ProposedMemberMapping
                {
                  From = property,
                  To = match,
                  IsEnumerable = true
                }
              );
            }
            else
            {

            }

            
          }
          else
          {
            var complexPair = new TypePair(property.PropertyType, match.PropertyType);

            ProposedTypeMapping complexTypeMapping;

            if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
            {
              complexTypeMapping = GetTypeMapping(complexPair, options);
            }

            complexTypeMapping.DestinationMember = match;
            complexTypeMapping.SourceMember = property;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
          }
        }
      }

      mappingCache.Add(pair, typeMapping);

      return typeMapping;
    }

    public IProposedMap CreateMap(TypePair pair, Action<PropertyInfo, IMappingOption> options = null)
    {

      var map = new ProposedMap(this.Mapper);

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      var mapping = GetTypeMapping(pair, options);

      map.ProposedTypeMapping = mapping;

      return map;

    }

    public IMemberMap CreateAndFinalizeMap(TypePair pair, Action<PropertyInfo, IMappingOption> options = null)
    {
      return CreateMap(pair, options).FinalizeMap();
    }

  }
}
