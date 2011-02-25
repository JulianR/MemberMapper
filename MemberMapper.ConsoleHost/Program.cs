using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;

namespace MemberMapper.ConsoleHost
{

  class SourceElement
  {
    public int X { get; set; }
  }

  class DestinationElement
  {
    public int X { get; set; }
  }

  class SourceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public List<SourceElement> IDs { get; set; }
  }

  class DestinationType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public List<DestinationElement> IDs { get; set; }
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
            X = 10
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
