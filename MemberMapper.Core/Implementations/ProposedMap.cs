using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace MemberMapper.Core.Implementations
{
  public class ProposedMap : IProposedMap
  {

    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public IMemberMapper Mapper { get; set; }

    public ProposedMap(IMemberMapper mapper)
    {
      Mapper = mapper;
    }

    private static void BuildTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping typeMapping, List<Expression> expressions)
    {
      foreach (var member in typeMapping.ProposedMappings)
      {
        var sourceMember = Expression.PropertyOrField(source, member.From.Name);
        var destMember = Expression.PropertyOrField(destination, member.To.Name);

        var assignSourceToDest = Expression.Assign(destination, source);

        expressions.Add(assignSourceToDest);

      }

      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {

        ParameterExpression complexSource = null, complexDest = null;

        if (complexTypeMapping.SourceProperty.MemberType == MemberTypes.Property)
        {
          complexSource = Expression.Parameter(complexTypeMapping.SourceProperty.MemberType, "test");
        }
        else if (complexTypeMapping.SourceProperty.MemberType == MemberTypes.Field)
        {

        }

        var complexSource = Expression.Parameter(complexTypeMapping.SourceProperty.MemberType, "test");
        var complexDest = Expression.Parameter(complexTypeMapping.DestinationProperty.MemberType, "test");

        expressions.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.MemberType)));
        expressions.Add(Expression.Assign(complexDest, Expression.New(complexTypeMapping.DestinationProperty.MemberType)));

        BuildTypeMappingExpressions(complexSource, complexDest, complexTypeMapping, expressions);

      }


    }

    public IMemberMap FinalizeMap()
    {
      var left = Expression.Parameter(DestinationType, "left");
      var right = Expression.Parameter(SourceType, "right");

      var assignments = new List<Expression>();

      BuildTypeMappingExpressions(right, left, this.ProposedTypeMapping, assignments);

      var block = Expression.Block(assignments);

      var lambda = Expression.Lambda
      (
        typeof(Func<,,>).MakeGenericType(this.SourceType, this.DestinationType, this.DestinationType),
        block,
        right, left
      );

      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = lambda.Compile();

      Mapper.RegisterMap(map);

      return map;
    }

    public IProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap, IProposedMap<TSource, TDestination>
  {

    public ProposedMap() : base(null) { }

    #region IProposedMap<TSource,TDestination> Members

    public IProposedMap<TSource, TDestination> AddExpression(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
