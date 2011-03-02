﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MemberMapper.Core.Implementations;

namespace MemberMapper.Test
{
  [TestClass]
  public class DefaultMemberMapperEnumerableTests
  {

    private class SourceElement
    {
      public string Value { get; set; }
    }

    private class DestElement
    {
      public string Value { get; set; }
    }

    private class SourceListType
    {
      public List<SourceElement> List { get; set; }
    }

    private class DestinationListType
    {
      public List<DestElement> List { get; set; }
    }

    private class SourceIListType
    {
      public IList<SourceElement> List { get; set; }
    }

    private class DestinationIListType
    {
      public IList<DestElement> List { get; set; }
    }

    private class SourceEnumerableType
    {
      public IEnumerable<SourceElement> List { get; set; }
    }

    private class DestinationEnumerableType
    {
      public IEnumerable<DestElement> List { get; set; }
    }

    private class SourceArrayType
    {
      public SourceElement[] List { get; set; }
    }

    private class DestinationArrayType
    {
      public DestElement[] List { get; set; }
    }

    private class SourceSimpleListType
    {
      public List<string> List { get; set; }
    }

    private class DestinationSimpleListType
    {
      public List<string> List { get; set; }
    }

    private class SourceSimpleIListType
    {
      public List<string> List { get; set; }
    }

    private class DestinationSimpleIListType
    {
      public List<string> List { get; set; }
    }

    private class SourceSimpleArrayType
    {
      public string[] List { get; set; }
    }

    private class DestinationSimpleArrayType
    {
      public string[] List { get; set; }
    }

    [TestMethod]
    public void ListToListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void ListToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void IListToListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void IListToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void ListToEnumerableIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void IListToEnumerableIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToEnumerableIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToArrayIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToEnumerableIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void IListToArrayIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ListToArrayIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToArrayIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);
    }

    [TestMethod]
    public void SimpleListToListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceSimpleListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleListType, DestinationSimpleListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleListToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceSimpleListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleListType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleIListToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceSimpleIListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleIListType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleArrayToIListIsMappedCorrectly()
    {

      var mapper = new DefaultMemberMapper();

      var source = new SourceSimpleArrayType
      {
        List = new string[]
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleArrayType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }
  }
}