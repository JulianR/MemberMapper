using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

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

    private int currentID = 1;

    private string GetParameterName(PropertyOrFieldInfo member)
    {
      return member.Name + "#" + currentID++;
    }

    private string GetCollectionName()
    {
      return "collection#" + currentID++;
    }

    private string GetEnumeratorName()
    {
      return "enumerator#" + currentID++;
    }

    private string GetCollectionElementName()
    {
      return "item#" + currentID++;
    }

    private string GetIteratorVarName()
    {
      return "i#" + currentID++;
    }

    private void BuildTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping typeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      foreach (var member in typeMapping.ProposedMappings)
      {
        BuildSimpleTypeMappingExpressions(source, destination, member, expressions, newParams);
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

    private void BuildSimpleTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedMemberMapping member, List<Expression> expressions, List<ParameterExpression> newParams)
    {
      var sourceMember = Expression.PropertyOrField(source, member.From.Name);
      var destMember = Expression.PropertyOrField(destination, member.To.Name);

      var assignSourceToDest = Expression.Assign(destMember, sourceMember);

      expressions.Add(assignSourceToDest);
    }

    private void BuildCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      var ifNotNullBlock = new List<Expression>();

      var destinationCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(complexTypeMapping.DestinationMember);

      var sourceCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(complexTypeMapping.SourceMember);

      var sourceElementSameAsDestination = destinationCollectionElementType == sourceCollectionElementType;

      Type destinationCollectionType;
      ParameterExpression destinationCollection;

      var accessSourceCollection = Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember);

      Expression accessSourceCollectionSize;

      if (complexTypeMapping.SourceMember.PropertyOrFieldType.IsArray)
      {
        accessSourceCollectionSize = Expression.Property(accessSourceCollection, "Length");
      }
      else
      {

        var genericCollection = typeof(ICollection<>).MakeGenericType(sourceCollectionElementType);

        if (genericCollection.IsAssignableFrom(complexTypeMapping.SourceMember.PropertyOrFieldType))
        {
          var countProperty = genericCollection.GetProperty("Count");

          accessSourceCollectionSize = Expression.Property(accessSourceCollection, countProperty);
        }
        else
        {

          var countMethod = (from m in typeof(Enumerable).GetMethods()
                             where m.Name == "Count" && m.IsGenericMethod
                             && m.GetParameters().Length == 1
                             select m).FirstOrDefault();

          var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(sourceCollectionElementType);

          accessSourceCollectionSize = Expression.Call(null, countMethod.MakeGenericMethod(sourceCollectionElementType), accessSourceCollection);
        }
      }

      if (complexTypeMapping.DestinationMember.PropertyOrFieldType.IsArray)
      {
        destinationCollectionType = complexTypeMapping.DestinationMember.PropertyOrFieldType;

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        var createDestinationCollection = Expression.New(destinationCollectionType.GetConstructors().Single(), accessSourceCollectionSize);

        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }
      else
      {
        destinationCollectionType = typeof(List<>).MakeGenericType(destinationCollectionElementType);

        var createDestinationCollection = Expression.New(destinationCollectionType);

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }

      var sourceCollectionItem = Expression.Parameter(sourceCollectionElementType, GetCollectionElementName());

      var expressionsInsideLoop = new List<Expression>();

      var destinationCollectionItem = Expression.Parameter(destinationCollectionElementType, GetCollectionElementName());

      BinaryExpression assignNewItemToDestinationItem;

      if (!sourceElementSameAsDestination)
      {
        var createNewDestinationCollectionItem = Expression.New(destinationCollectionElementType);
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);
      }
      else
      {
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, sourceCollectionItem);
      }
      newParams.Add(sourceCollectionItem);
      newParams.Add(destinationCollectionItem);

      var @break = Expression.Label();

      var iteratorVar = Expression.Parameter(typeof(int), GetIteratorVarName());

      var increment = Expression.PostIncrementAssign(iteratorVar);

      Expression assignItemToDestination;

      if (destinationCollectionType.IsArray)
      {
        var accessDestinationCollectionByIndex = Expression.MakeIndex(destinationCollection, null, new[] { iteratorVar });

        var assignDestinationItemToArray = Expression.Assign(accessDestinationCollectionByIndex, destinationCollectionItem);

        assignItemToDestination = assignDestinationItemToArray;
      }
      else
      {
        var addMethod = destinationCollectionType.GetMethod("Add", new[] { destinationCollectionElementType });
        var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

        assignItemToDestination = callAddOnDestinationCollection;

      }

      if (typeof(IList).IsAssignableFrom(complexTypeMapping.SourceMember.PropertyOrFieldType)
        || (complexTypeMapping.SourceMember.PropertyOrFieldType.IsGenericType && typeof(IList<>).IsAssignableFrom(complexTypeMapping.SourceMember.PropertyOrFieldType.GetGenericTypeDefinition())))
      {


        newParams.Add(iteratorVar);

        var assignZeroToIteratorVar = Expression.Assign(iteratorVar, Expression.Constant(0));

        var terminationCondition = Expression.LessThan(iteratorVar, accessSourceCollectionSize);

        var indexer = complexTypeMapping.SourceMember.PropertyOrFieldType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);

        var accessSourceCollectionByIndex = Expression.MakeIndex(accessSourceCollection, indexer, new[] { iteratorVar });

        var assignCurrent = Expression.Assign(sourceCollectionItem, accessSourceCollectionByIndex);

        expressionsInsideLoop.Add(assignCurrent);

        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams);

        expressionsInsideLoop.Add(assignItemToDestination);

        expressionsInsideLoop.Add(increment);

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        var @foreach = Expression.Loop(
                        Expression.IfThenElse(
                        terminationCondition,
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        ifNotNullBlock.Add(@foreach);
      }
      else
      {

        var getEnumeratorOnSourceMethod = complexTypeMapping.SourceMember.PropertyOrFieldType.GetMethod("GetEnumerator", Type.EmptyTypes);

        var sourceEnumeratorType = getEnumeratorOnSourceMethod.ReturnType;

        var sourceEnumerator = Expression.Parameter(sourceEnumeratorType, GetEnumeratorName());

        if (complexTypeMapping.DestinationMember.PropertyOrFieldType.IsArray)
        {
          newParams.Add(iteratorVar);
        }

        newParams.Add(sourceEnumerator);

        var doMoveNextCall = Expression.Call(sourceEnumerator, typeof(IEnumerator).GetMethod("MoveNext"));

        var assignToEnum = Expression.Assign(sourceEnumerator, Expression.Call(accessSourceCollection, getEnumeratorOnSourceMethod));

        ifNotNullBlock.Add(assignToEnum);

        var assignCurrent = Expression.Assign(sourceCollectionItem, Expression.Property(sourceEnumerator, "Current"));

        expressionsInsideLoop.Add(assignCurrent);
        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams);

        expressionsInsideLoop.Add(assignItemToDestination);

        if (complexTypeMapping.DestinationMember.PropertyOrFieldType.IsArray)
        {
          expressionsInsideLoop.Add(increment);
        }

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        var @foreach = Expression.Loop(
                        Expression.IfThenElse(
                        Expression.NotEqual(doMoveNextCall, Expression.Constant(false)),
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        ifNotNullBlock.Add(@foreach);

      }

      var accessDestinationCollection = Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember);

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      ifNotNullBlock.Add(assignDestinationCollection);

      var ifNotNullCheck = Expression.IfThen(Expression.NotEqual(accessSourceCollection, Expression.Constant(null)), Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

    }

    private void BuildNonCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, IProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
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

    private static ModuleBuilder moduleBuilder;

    private static Delegate CompileExpression(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      if ((sourceType.IsPublic || sourceType.IsNestedPublic) && (destinationType.IsPublic || destinationType.IsNestedPublic))
      {
        if (moduleBuilder == null)
        {
          var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("MemberMapperFunctionsAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);

          moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
        }

        var typeBuilder = moduleBuilder.DefineType(string.Format("From_{0}_to_{1}_{2}", sourceType.Name, destinationType.Name, Guid.NewGuid().ToString("N"), TypeAttributes.Public));

        var methodBuilder = typeBuilder.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Static);

        expression.CompileToMethod(methodBuilder);

        var resultingType = typeBuilder.CreateType();

        var function = Delegate.CreateDelegate(expression.Type, resultingType.GetMethod("Map"));

        return function;
      }
      else
      {
        return expression.Compile();
      }

    }

    public IMemberMap FinalizeMap()
    {
      var left = Expression.Parameter(DestinationType, "left");
      var right = Expression.Parameter(SourceType, "right");

      var assignments = new List<Expression>();

      var newParams = new List<ParameterExpression>();

      BuildTypeMappingExpressions(right, left, this.ProposedTypeMapping, assignments, newParams);

      var block = Expression.Block(assignments);

      var sourceNullCheck = Expression.NotEqual(right, Expression.Constant(null));

      var ifSourceNotNull = Expression.IfThen(sourceNullCheck, block);

      var outerBlock = Expression.Block(newParams, ifSourceNotNull, left);

      var funcType = typeof(Func<,,>).MakeGenericType(this.SourceType, this.DestinationType, this.DestinationType);

      var lambda = Expression.Lambda
      (
        funcType,
        outerBlock,
        right, left
      );

      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = CompileExpression(this.SourceType, this.DestinationType, lambda);

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
    public IProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      throw new NotImplementedException();
    }
  }
}
