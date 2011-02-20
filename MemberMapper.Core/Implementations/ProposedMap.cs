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

    private static string GetParameterName(MemberInfo member)
    {
      return member.Name + "$" + ((uint)Guid.NewGuid().GetHashCode());
    }

    private static void BuildTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping typeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {
      foreach (var member in typeMapping.ProposedMappings)
      {
        var sourceMember = Expression.PropertyOrField(source, member.From.Name);
        var destMember = Expression.PropertyOrField(destination, member.To.Name);

        var assignSourceToDest = Expression.Assign(destMember, sourceMember);

        expressions.Add(assignSourceToDest);
      }


      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {

        var ifNotNullBlock = new List<Expression>();

        ParameterExpression complexSource = null, complexDest = null;

        if (complexTypeMapping.SourceMember.MemberType == MemberTypes.Property)
        {
          complexSource = Expression.Parameter(((PropertyInfo)complexTypeMapping.SourceMember).PropertyType, GetParameterName(complexTypeMapping.SourceMember));
        }
        else if (complexTypeMapping.SourceMember.MemberType == MemberTypes.Field)
        {
          complexSource = Expression.Parameter(((FieldInfo)complexTypeMapping.SourceMember).FieldType, GetParameterName(complexTypeMapping.SourceMember));
        }

        if (complexTypeMapping.DestinationMember.MemberType == MemberTypes.Property)
        {
          complexDest = Expression.Parameter(((PropertyInfo)complexTypeMapping.DestinationMember).PropertyType, GetParameterName(complexTypeMapping.DestinationMember));
        }
        else if (complexTypeMapping.DestinationMember.MemberType == MemberTypes.Field)
        {
          complexDest = Expression.Parameter(((FieldInfo)complexTypeMapping.DestinationMember).FieldType, GetParameterName(complexTypeMapping.DestinationMember));
        }

        newParams.Add(complexSource);
        newParams.Add(complexDest);

        ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.SourceMember.Name)));

        var newType = complexTypeMapping.DestinationMember.MemberType == MemberTypes.Property ? ((PropertyInfo)complexTypeMapping.DestinationMember).PropertyType : ((FieldInfo)complexTypeMapping.DestinationMember).FieldType;

        ifNotNullBlock.Add(Expression.Assign(complexDest, Expression.New(newType)));

        BuildTypeMappingExpressions(complexSource, complexDest, complexTypeMapping, ifNotNullBlock, newParams);

        ifNotNullBlock.Add(Expression.Assign(Expression.PropertyOrField(destination, complexTypeMapping.DestinationMember.Name), complexDest));

        var ifNotNullCheck = Expression.IfThen(Expression.NotEqual(Expression.Property(source, complexTypeMapping.SourceMember.Name), Expression.Constant(null)), Expression.Block(ifNotNullBlock));

        expressions.Add(ifNotNullCheck);


      }
    }

    public IMemberMap FinalizeMap()
    {
      var left = Expression.Parameter(DestinationType, "left");
      var right = Expression.Parameter(SourceType, "right");

      var assignments = new List<Expression>();

      var newParams = new List<ParameterExpression>();

      BuildTypeMappingExpressions(right, left, this.ProposedTypeMapping, assignments, newParams);

      assignments.Add(left);

      var block = Expression.Block(newParams, assignments);

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
