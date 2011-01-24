using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;

namespace MemberMapper.Core.Implementations
{
  public class MemberMap : IMemberMap
  {
    public IMemberMap FinalizeMap()
    {
      return this;
    }

    public IProposedTypeMapping ProposedTypeMapping
    {
      get { throw new NotImplementedException(); }
    }

    #region IMemberMap Members

    public Type SourceType
    {
      get;
      set;
    }

    public Type DestinationType
    {
      get;
      set;
    }

    public Delegate MappingFunction
    {
      get;
      set;
    }

    #endregion
  }
}
