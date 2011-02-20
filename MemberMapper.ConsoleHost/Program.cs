using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;

namespace MemberMapper.ConsoleHost
{
  class SourceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public List<int> IDs { get; set; }
  }

  class DestinationType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public List<int> IDs { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      //var mapper = new DefaultMappingStrategy();

      //var map = mapper.CreateMap(new TypePair(typeof(SourceType), typeof(DestinationType)));

      //map.FinalizeMap();

      new ProposedMap<SourceType, DestinationType>().AddExpression(source => source.ID, destination => destination.ID);

    }

    static void Foo()
    {

    }
  }
}
