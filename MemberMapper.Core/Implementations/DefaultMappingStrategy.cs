using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

namespace MemberMapper.Core.Implementations
{
  public class DefaultMappingStrategy : IMappingStrategy
  {
    private Dictionary<TypePair, ProposedTypeMapping> mappingCache = new Dictionary<TypePair, ProposedTypeMapping>();

    private byte[] syncRoot = new byte[0];


    public IMapGenerator MapGenerator { get; set; }

    private void HandleMappingForMembers(ProposedTypeMapping typeMapping, PropertyOrFieldInfo property, PropertyOrFieldInfo match, MappingOptions options = null)
    {
      if (match != null && match.PropertyOrFieldType.IsAssignableFrom(property.PropertyOrFieldType))
      {

        if (options != null)
        {
          var option = new MappingOption();

          options(property, match, option);

          switch (option.State)
          {
            case MappingOptionState.Ignored:
              return;
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

        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyOrFieldType)
          && typeof(IEnumerable).IsAssignableFrom(match.PropertyOrFieldType))
        {

          var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(property);
          var typeOfDestinationEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(match);

          if (typeOfDestinationEnumerable == typeOfSourceEnumerable)
          {

            typeMapping.ProposedTypeMappings.Add(
              new ProposedTypeMapping
              {
                IsEnumerable = true,
                DestinationMember = match,
                SourceMember = property,
                ProposedMappings = new List<IProposedMemberMapping>()
              });
          }
          else
          {
            var complexPair = new TypePair(typeOfSourceEnumerable, typeOfDestinationEnumerable);

            ProposedTypeMapping complexTypeMapping;

            if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
            {
              complexTypeMapping = GetTypeMapping(complexPair, options);
            }
            complexTypeMapping.IsEnumerable = true;
            complexTypeMapping.DestinationMember = match;
            complexTypeMapping.SourceMember = property;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
          }


        }
        else
        {
          var complexPair = new TypePair(property.PropertyOrFieldType, match.PropertyOrFieldType);

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

    // ThisMemberment

    private ProposedTypeMapping GetTypeMapping(TypePair pair, MappingOptions options = null, Expression customMapping = null)
    {
      var typeMapping = new ProposedTypeMapping();

      typeMapping.SourceMember = null;
      typeMapping.DestinationMember = null;

      var destinationProperties = (from p in pair.DestinationType.GetProperties()
                                   where p.CanWrite
                                   select p);

      HashSet<string> customProperties = new HashSet<string>();

      if (customMapping != null)
      {
        var lambda = customMapping as LambdaExpression;

        if (lambda == null) throw new ArgumentException("Only LambdaExpression is allowed here");

        var newType = lambda.Body as NewExpression;

        if (newType == null) throw new ArgumentException("Only NewExpression is allowed to specify a custom mapping");

        customProperties = new HashSet<string>(newType.Members.Select(m => m.Name));

        foreach (var member in newType.Members)
        {
          PropertyInfo prop;
          //if (destinationProperties.TryGetValue(member.Name, out prop))
          //{
          //  Console.WriteLine(prop);
          //}
        }

      }

      
      var sourceProperties = (from p in pair.SourceType.GetProperties()
                              where p.CanRead
                              select p).ToDictionary(k => k.Name);

      foreach (var property in destinationProperties)
      {
        PropertyInfo match;

        if (customProperties.Contains(property.Name))
        {
          continue;
        }

        if (sourceProperties.TryGetValue(property.Name, out match)
          && property.PropertyType.IsAssignableFrom(match.PropertyType))
        {

          if (options != null)
          {
            var option = new MappingOption();

            options(match, property, option);

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
              From = match,
              To = property
            }
          );
        }
        else if (match != null)
        {

          if (typeof(IEnumerable).IsAssignableFrom(match.PropertyType)
            && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
          {

            var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(match);
            var typeOfDestinationEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(property);

            if (typeOfDestinationEnumerable == typeOfSourceEnumerable)
            {

              typeMapping.ProposedTypeMappings.Add(
                new ProposedTypeMapping
              {
                IsEnumerable = true,
                DestinationMember = property,
                SourceMember = match,
                ProposedMappings = new List<IProposedMemberMapping>()
              });

            }
            else
            {
              var complexPair = new TypePair(typeOfSourceEnumerable, typeOfDestinationEnumerable);

              ProposedTypeMapping complexTypeMapping;

              if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
              {
                complexTypeMapping = GetTypeMapping(complexPair, options);
              }

              complexTypeMapping = complexTypeMapping.Clone();

              complexTypeMapping.IsEnumerable = true;
              complexTypeMapping.DestinationMember = property;
              complexTypeMapping.SourceMember = match;

              typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
            }


          }
          else
          {
            var complexPair = new TypePair(match.PropertyType, property.PropertyType);

            ProposedTypeMapping complexTypeMapping;

            if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
            {
              complexTypeMapping = GetTypeMapping(complexPair, options);
            }

            complexTypeMapping = complexTypeMapping.Clone();

            complexTypeMapping.DestinationMember = property;
            complexTypeMapping.SourceMember = match;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
          }
        }
      }

      lock (syncRoot)
      {
        mappingCache.Add(pair, typeMapping);
      }

      return typeMapping;
    }

    public event Action<IMemberMap> MemberMapCreated;

    private void RegisterMap(ProposedMap proposed, IMemberMap final)
    {

      if (MemberMapCreated != null)
      {
        MemberMapCreated(final);
      }

      proposed.MemberMapCreated -= RegisterMap;
    }



    public IProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      var map = new ProposedMap<TSource, TDestination>();

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      map.MemberMapCreated += RegisterMap;

      
      map.MapGenerator = this.MapGenerator;


      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      var mapping = GetTypeMapping(pair, options, customMapping);

      map.ProposedTypeMapping = mapping;

      return map;
    }

    public IProposedMap CreateMap(TypePair pair, MappingOptions options = null)
    {

      var map = new ProposedMap();

      map.MapGenerator = this.MapGenerator;

      map.MemberMapCreated += RegisterMap;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      var mapping = GetTypeMapping(pair, options);

      map.ProposedTypeMapping = mapping;

      return map;

    }

    public IMemberMap CreateAndFinalizeMap(TypePair pair, MappingOptions options = null)
    {
      return CreateMap(pair, options).FinalizeMap();
    }

  }
}
