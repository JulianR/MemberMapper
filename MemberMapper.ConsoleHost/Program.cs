using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;

namespace MemberMapper.ConsoleHost
{

  class Foo
  {
    public string Z {get;set;}
  }

  class Bar
  {
    public string Z { get;set;}
  }

  class SourceElement
  {
    public int X { get; set; }

    public List<Foo> Collection { get; set; }
  }

  class DestinationElement
  {
    public int X { get; set; }

    public List<Bar> Collection { get; set; }
  }

  class SourceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public IList<SourceElement> IDs { get; set; }
  }

  class DestinationType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public IEnumerable<DestinationElement> IDs { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      var mapper = new DefaultMemberMapper();

      var map = mapper.CreateMap(typeof(SourceType), typeof(DestinationType)).FinalizeMap();

      var source = new SourceType
      {
        ID = 1,
        IDs = new List<SourceElement>
        {
          new SourceElement
          {
            X = 10,
            Collection = new List<Foo>
            {
              new Foo
              {
                Z = "string"
              }
            }
          }
        },
        Name = "X"
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      //map.FinalizeMap();

      //new ProposedMap<SourceType, DestinationType>().AddExpression(source => source.ID, destination => destination.ID);

    }

    static void Foo()
    {

    }
  }
}
