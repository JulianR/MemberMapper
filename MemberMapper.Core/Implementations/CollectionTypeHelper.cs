using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MemberMapper.Core.Implementations
{
  public static class CollectionTypeHelper
  {
    public static Type GetTypeInsideEnumerable(PropertyOrFieldInfo prop)
    {
      var getEnumeratorMethod = prop.PropertyOrFieldType.GetMethod("GetEnumerator", Type.EmptyTypes);

      if (getEnumeratorMethod == null) return null;

      if (getEnumeratorMethod.ReturnType.IsGenericType)
      {
        return getEnumeratorMethod.ReturnType.GetGenericArguments().First();
      }

      if (prop.PropertyOrFieldType.IsArray)
      {
        return prop.PropertyOrFieldType.GetElementType();
      }

      return typeof(object);

    }
  }
}
