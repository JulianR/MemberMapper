using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MemberMapper.Core.Implementations;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedMemberMapping
  {
    PropertyOrFieldInfo From { get; set; }
    PropertyOrFieldInfo To { get; set; }
    bool IsEnumerable { get; set; }
  }
}
