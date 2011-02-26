﻿using System;
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

      if (getEnumeratorMethod == null)
      {
        getEnumeratorMethod = (from i in prop.PropertyOrFieldType.GetInterfaces()
                               from m in i.GetMethods()
                               where m.Name == "GetEnumerator"
                               orderby m.ReturnType.IsGenericType descending
                               select m).FirstOrDefault();
                               
      }

      if (getEnumeratorMethod == null) return null;

      if (getEnumeratorMethod.ReturnType.IsGenericType)
      {
        return getEnumeratorMethod.ReturnType.GetGenericArguments().First();
      }
      else if (prop.PropertyOrFieldType.IsArray)
      {
        return prop.PropertyOrFieldType.GetElementType();
      }

      return typeof(object);

    }
  }
}
