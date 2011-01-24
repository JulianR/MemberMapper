using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;

namespace MemberMapper.Core.Implementations
{

  public enum MappingOptionState
  {
    Default,
    Ignored
  }

  public class MappingOption : IMappingOption
  {

    public MappingOptionState State { get; private set; }

    public void IgnoreMember()
    {
      State = MappingOptionState.Ignored;
    }

  }
}
