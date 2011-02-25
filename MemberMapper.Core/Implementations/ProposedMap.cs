using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace MemberMapper.Core.Implementations
{
  public class ProposedMap : IProposedMap
  {

    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public ProposedMap()
    {
    }

    public event Action<ProposedMap, IMemberMap> MemberMapCreated;

    private static string GetParameterName(PropertyOrFieldInfo member)
    {
      return member.Name + "#" + ((uint)Guid.NewGuid().GetHashCode());
    }

    private static string GetCollectionName()
    {
      return "collection#" + ((uint)Guid.NewGuid().GetHashCode());
    }

    private static string GetEnumeratorName()
    {
      return "enumerator#" + ((uint)Guid.NewGuid().GetHashCode());
    }

    private static string GetCollectionElementName()
    {
      return "item#" + ((uint)Guid.NewGuid().GetHashCode());
    }

    private static void BuildTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping typeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      foreach (var member in typeMapping.ProposedMappings)
      {
        if (member.IsEnumerable)
        {

        }
        else
        {
          BuildNonCollectionSimpleTypeMappingExpressions(source, destination, member, expressions, newParams);
        }
      }

      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {
        if (complexTypeMapping.IsEnumerable)
        {
          BuildCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions, newParams);
        }
        else
        {
          BuildNonCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions, newParams);
        }
      }


    }

    private static void BuildNonCollectionSimpleTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedMemberMapping member, List<Expression> expressions, List<ParameterExpression> newParams)
    {
      var sourceMember = Expression.PropertyOrField(source, member.From.Name);
      var destMember = Expression.PropertyOrField(destination, member.To.Name);

      var assignSourceToDest = Expression.Assign(destMember, sourceMember);

      expressions.Add(assignSourceToDest);
    }

    private static void BuildCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      var destinationCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(complexTypeMapping.DestinationMember);

      var sourceCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(complexTypeMapping.SourceMember);

      var destinationCollectionType = typeof(List<>).MakeGenericType(destinationCollectionElementType);

      var createDestinationCollection = Expression.New(destinationCollectionType);

      var destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

      newParams.Add(destinationCollection);

      var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

      expressions.Add(assignNewCollectionToDestination);

      var getEnumeratorOnSourceMethod = complexTypeMapping.SourceMember.PropertyOrFieldType.GetMethod("GetEnumerator", Type.EmptyTypes);

      var sourceEnumeratorType = getEnumeratorOnSourceMethod.ReturnType;

      var sourceEnumerator = Expression.Parameter(sourceEnumeratorType, GetEnumeratorName());

      newParams.Add(sourceEnumerator);

      var doMoveNextCall = Expression.Call(sourceEnumerator, sourceEnumeratorType.GetMethod("MoveNext"));

      var accessSourceCollection = Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember);

      var assignToEnum = Expression.Assign(sourceEnumerator, Expression.Call(accessSourceCollection, getEnumeratorOnSourceMethod));

      var sourceCollectionItem = Expression.Parameter(sourceCollectionElementType, GetCollectionElementName());

      var destinationCollectionItem = Expression.Parameter(destinationCollectionElementType, GetCollectionElementName());

      var createNewDestinationCollectionItem = Expression.New(destinationCollectionElementType);

      var assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);

      newParams.Add(sourceCollectionItem);
      newParams.Add(destinationCollectionItem);

      var assignCurrent = Expression.Assign(sourceCollectionItem, Expression.Property(sourceEnumerator, "Current"));

      var expressionsInsideLoop = new List<Expression>();

      expressionsInsideLoop.Add(assignCurrent);
      expressionsInsideLoop.Add(assignNewItemToDestinationItem);

      var @break = Expression.Label();

      BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams);

      var addMethod = destinationCollectionType.GetMethod("Add", new[] { destinationCollectionElementType });

      var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

      expressionsInsideLoop.Add(callAddOnDestinationCollection);

      var blockInsideLoop = Expression.Block(expressionsInsideLoop);

      var @foreach = Expression.Block(
          assignToEnum,
          Expression.Loop(
              Expression.IfThenElse(
              Expression.NotEqual(doMoveNextCall, Expression.Constant(false)),
                  blockInsideLoop
              , Expression.Break(@break))
          , @break)
      );

      expressions.Add(@foreach);

      var accessDestinationCollection = Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember);

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      expressions.Add(assignDestinationCollection);

    }

    private static void BuildNonCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      ParameterExpression complexSource = null, complexDest = null;

      complexSource = Expression.Parameter(complexTypeMapping.SourceMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.SourceMember));

      complexDest = Expression.Parameter(complexTypeMapping.DestinationMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.DestinationMember));

      newParams.Add(complexSource);
      newParams.Add(complexDest);

      var ifNotNullBlock = new List<Expression>();

      ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.SourceMember.Name)));

      var newType = complexTypeMapping.DestinationMember.PropertyOrFieldType;

      ifNotNullBlock.Add(Expression.Assign(complexDest, Expression.New(newType)));

      BuildTypeMappingExpressions(complexSource, complexDest, complexTypeMapping, ifNotNullBlock, newParams);

      ifNotNullBlock.Add(Expression.Assign(Expression.PropertyOrField(destination, complexTypeMapping.DestinationMember.Name), complexDest));

      var ifNotNullCheck = Expression.IfThen(Expression.NotEqual(Expression.Property(source, complexTypeMapping.SourceMember.Name), Expression.Constant(null)), Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

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

      if (MemberMapCreated != null)
      {
        MemberMapCreated(this, map);
      }

      return map;
    }

    public IProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap, IProposedMap<TSource, TDestination>
  {

    #region IProposedMap<TSource,TDestination> Members

    public IProposedMap<TSource, TDestination> AddExpression(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
