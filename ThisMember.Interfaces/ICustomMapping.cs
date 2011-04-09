using System;
namespace ThisMember.Interfaces
{
  public interface ICustomMapping
  {
    System.Collections.Generic.IList<ICustomMapping> CustomMappings { get; set; }
    Type DestinationType { get; set; }
    System.Linq.Expressions.Expression GetExpressionForMember(PropertyOrFieldInfo member);
    System.Collections.Generic.IList<MemberExpressionTuple> Members { get; set; }
  }
}
