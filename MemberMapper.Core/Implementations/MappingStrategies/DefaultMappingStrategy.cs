﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Reflection;

namespace MemberMapper.Core.Implementations.MappingStrategies
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

    private static ProposedTypeMapping GetTypeMapping(TypePair pair, Action<PropertyInfo, IMappingOption> options = null)
    {


      var typeMapping = new ProposedTypeMapping();

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
          typeMapping.ProposedTypeMappings.Add(GetTypeMapping(new TypePair(property.PropertyType, match.PropertyType), options));
        }
      }

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
