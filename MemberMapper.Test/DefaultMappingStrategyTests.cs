using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MemberMapper.Core.Implementations;
using System.Linq.Expressions;
using MemberMapper.Core.Interfaces;
using System.Reflection;

namespace MemberMapper.Test
{
  [TestClass]
  public class DefaultMappingStrategyTests
  {

    private class Poco_From
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public List<int> OtherIDs { get; set; }
    }

    private class Poco_To
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public List<int> OtherIDs { get; set; }
    }

    private class ExpectedMappings<T, T1>
    {

      public List<ExpectedMapping<T, T1>> Mappings { get; set; }

      public ExpectedMappings()
      {
        Mappings = new List<ExpectedMapping<T, T1>>();
      }

      public void Add(Expression<Func<T, object>> left, Expression<Func<T1, object>> right)
      {
        Mappings.Add(new ExpectedMapping<T, T1>(left, right));
      }
    }

    private class ExpectedMapping<T, T1>
    {
      public Expression<Func<T, object>> Left { get; set; }
      public Expression<Func<T1, object>> Right { get; set; }

      public ExpectedMapping(Expression<Func<T, object>> left, Expression<Func<T1, object>> right)
      {
        Left = left;
        Right = right;
      }

    }

    private static PropertyOrFieldInfo GetMemberInfoFromExpression(Expression body)
    {
      if ((body != null) && ((body.NodeType == ExpressionType.Convert) || (body.NodeType == ExpressionType.ConvertChecked)))
      {
        body = ((UnaryExpression)body).Operand;
      }
      var expression2 = (MemberExpression)body;
      return expression2.Member;
    }

    static bool ContainsMappingFor<T, T2>(IProposedMap map, ExpectedMappings<T, T2> mappings)
    {
      int foundMappings = 0;
      foreach (var mapping in mappings.Mappings)
      {

        if (!map.ProposedTypeMapping.ProposedMappings.Contains(
        new ProposedMemberMapping()
        {
          From = GetMemberInfoFromExpression(mapping.Left.Body),
          To = GetMemberInfoFromExpression(mapping.Right.Body)
        }))
        {
          return false;
        }
        else
        {
          foundMappings++;
        }

      }

      if (map.ProposedTypeMapping.ProposedMappings.Count != foundMappings)
      {
        return false;
      }

      return true;
    }

    [TestMethod]
    public void ExpectedPropertiesWillBeMapped()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.MappingStrategy.CreateMap(new TypePair(typeof(Poco_From), typeof(Poco_To)));

      proposition.FinalizeMap();

      var expectation = new ExpectedMappings<Poco_From, Poco_To>();

      expectation.Add(t => t.ID, t => t.ID);
      expectation.Add(t => t.Name, t => t.Name);
      expectation.Add(t => t.OtherIDs, t => t.OtherIDs);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );


    }

    private interface IPocoOneProperty_From
    {
      int ID { get; set; }
    }

    private class PocoOneProperty_From : IPocoOneProperty_From
    {
      public int ID { get; set; }
    }

    private class PocoOneProperty_To
    {
      public int ID { get; set; }
    }

    [TestMethod]
    public void ExpectedPropertiesFromInterfaceWillBeMapped()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.MappingStrategy.CreateMap(new TypePair(typeof(IPocoOneProperty_From), typeof(PocoOneProperty_To)));

      var expectation = new ExpectedMappings<IPocoOneProperty_From, PocoOneProperty_To>();

      expectation.Add(t => t.ID, t => t.ID);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );
    }

    [TestMethod]
    public void IgnoringMemberIgnoresTheMember()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.MappingStrategy.CreateMap(
        new TypePair(typeof(Poco_From), typeof(Poco_To)),
        (p, option) =>
        {
          if (p.Name == "ID")
          {
            option.IgnoreMember();
          }
        }
      );

      var expectation = new ExpectedMappings<Poco_From, Poco_To>();

      expectation.Add(t => t.Name, t => t.Name);
      expectation.Add(t => t.OtherIDs, t => t.OtherIDs);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );
    }

    [TestMethod]
    public void FinalFunctionIsOfExpectedType()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.MappingStrategy.CreateMap(new TypePair(typeof(IPocoOneProperty_From), typeof(PocoOneProperty_To)));

      var map = proposition.FinalizeMap();

      Assert.IsTrue(map.MappingFunction.GetType() == typeof(Func<IPocoOneProperty_From, PocoOneProperty_To, PocoOneProperty_To>));

    }

    private class ListDestinationType
    {
      public List<int> Source { get; set; }
    }

    private class ListSourceType
    {
      public List<int> Source { get; set; }
    }

    private class IEnumerableSourceType
    {
      public IEnumerable<int> Source { get; set; }
    }

    private class ArrayDestinationType
    {
      public int[] Source { get; set; }
    }

    [TestMethod]
    public void ListIntPropertyGetsNormalMapping()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.CreateMap(typeof(ListSourceType), typeof(ListDestinationType));

      Assert.IsTrue(proposition.ProposedTypeMapping.ProposedMappings.Any(p => !p.IsEnumerable));
    }

    [TestMethod]
    public void ListIntPropertyGetsMappedToIEnumerable()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.CreateMap(typeof(IEnumerableSourceType), typeof(ListDestinationType));

      Assert.IsTrue(proposition.ProposedTypeMapping.ProposedMappings.Any(p => p.IsEnumerable));

    }

    [TestMethod]
    public void ListIntPropertyGetsMappedToArray()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.CreateMap(typeof(IEnumerableSourceType), typeof(ArrayDestinationType));

      Assert.IsTrue(proposition.ProposedTypeMapping.ProposedMappings.Any(p => p.IsEnumerable));
    }

    private class SourceElementType
    {
      public int ID { get; set; }
    }

    private class ListComplexSourceType
    {
      public List<SourceElementType> Source { get; set; }
    }

    private class DestinationElementType
    {
      public int ID { get; set; }
    }

    private class ListComplexDestinationType
    {
      public List<DestinationElementType> Source { get; set; }
    }

    [TestMethod]
    public void ComplexListGetsMapped()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.CreateMap(typeof(ListComplexSourceType), typeof(ListComplexDestinationType));

      Assert.IsTrue(proposition.ProposedTypeMapping.ProposedTypeMappings.Any(p => p.IsEnumerable));
    }

    private class IEnumerableComplexSourceType
    {
      public List<SourceElementType> Source { get; set; }
    }

    [TestMethod]
    public void ComplexListGetsMappedFromIEnumerable()
    {
      var mapper = new DefaultMemberMapper(new DefaultMappingStrategy());

      var proposition = mapper.CreateMap(typeof(IEnumerableComplexSourceType), typeof(ListComplexDestinationType));

      Assert.IsTrue(proposition.ProposedTypeMapping.ProposedTypeMappings.Any(p => p.IsEnumerable));
    }

  }
}
