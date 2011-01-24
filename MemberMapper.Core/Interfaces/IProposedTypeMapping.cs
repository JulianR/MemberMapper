using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedTypeMapping
  {
    MemberInfo SourceProperty { get; set; }
    MemberInfo DestinationProperty { get; set; }

    IList<IProposedMemberMapping> ProposedMappings { get; }
    IList<IProposedTypeMapping> ProposedTypeMappings { get; }

  }
}
