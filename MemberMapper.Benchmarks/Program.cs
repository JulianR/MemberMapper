﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MemberMapper.Benchmarks
{
  class Program
  {
    static void Main(string[] args)
    {


      var param = Expression.Parameter(typeof(int), "i");

      var outerBlockParams = new List<ParameterExpression>();

      outerBlockParams.Add(param);

      var innerBlock = Expression.Block(Expression.Assign(param, Expression.Constant(1)));

      var outerBlock = Expression.Block(outerBlockParams, innerBlock);

      var lambda = Expression.Lambda<Action>(outerBlock);

      Benchmark();
      Console.WriteLine();
      Benchmark();

      //Foobar();
      //Console.WriteLine();
      //Foobar();
    }

    public class NestedSourceType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    public class ComplexSourceType
    {
      public int ID { get; set; }
      public NestedSourceType Complex { get; set; }
    }

    public class NestedDestinationType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    public class ComplexDestinationType
    {
      public int ID { get; set; }
      public NestedDestinationType Complex { get; set; }
    }

    public static ComplexDestinationType Foo;

    public static List<int> Bar = new List<int>();

    static void Foobar()
    {

      Func<int, List<int>> func = to1 =>
      {
        var res1 = new List<int>();
        for (int n1 = 2; n1 <= to1; n1++)
        {
          bool found1 = false;

          for (int d1 = 2; d1 <= Math.Sqrt(n1); d1++)
          {
            if (n1 % d1 == 0)
            {
              found1 = true;
              break;
            }
          }

          if (!found1)
            res1.Add(n1);
        }
        return res1;
      };

      var to = Expression.Parameter(typeof(int), "to");
      var res = Expression.Variable(typeof(List<int>), "res");
      var n = Expression.Variable(typeof(int), "n");
      var found = Expression.Variable(typeof(bool), "found");
      var d = Expression.Variable(typeof(int), "d");
      var breakOuter = Expression.Label();
      var breakInner = Expression.Label();
      var func1 = Expression.Lambda<Func<int, List<int>>>(
        // {
              Expression.Block(
        // List<int> res;
                  new[] { res },
        // res = new List<int>();
                  Expression.Assign(
                      res,
                      Expression.New(typeof(List<int>))
                  ),
        // {
                  Expression.Block(
        // int n;
                      new[] { n },
        // n = 2;
                      Expression.Assign(
                          n,
                          Expression.Constant(2)
                      ),
        // while (true)
                      Expression.Loop(
        // {
                          Expression.Block(
        // if
                              Expression.IfThen(
        // (!
                                  Expression.Not(
        // (n <= to)
                                      Expression.LessThanOrEqual(
                                          n,
                                          to
                                      )
        // )
                                  ),
        // break;
                                  Expression.Break(breakOuter)
                              ),
        // {
                              Expression.Block(
        // bool found;
                                  new[] { found },
        // found = false;
                                  Expression.Assign(
                                      found,
                                      Expression.Constant(false)
                                  ),
        // {
                                  Expression.Block(
        // int d;
                                      new[] { d },
        // d = 2;
                                      Expression.Assign(
                                          d,
                                          Expression.Constant(2)
                                      ),
        // while (true)
                                      Expression.Loop(
        // {
                                          Expression.Block(
        // if
                                              Expression.IfThen(
        // (!
                                                  Expression.Not(
        // d <= Math.Sqrt(n)
                                                      Expression.LessThanOrEqual(
                                                          d,
                                                          Expression.Convert(
                                                              Expression.Call(
                                                                  null,
                                                                  typeof(Math).GetMethod("Sqrt"),
                                                                  Expression.Convert(
                                                                      n,
                                                                      typeof(double)
                                                                  )
                                                              ),
                                                              typeof(int)
                                                          )
                                                      )
        // )
                                                  ),
        // break;
                                                  Expression.Break(breakInner)
                                              ),
        // {
                                              Expression.Block(
        // if (n % d == 0)
                                                  Expression.IfThen(
                                                      Expression.Equal(
                                                          Expression.Modulo(
                                                              n,
                                                              d
                                                          ),
                                                          Expression.Constant(0)
                                                      ),
        // {
                                                      Expression.Block(
        // found = true;
                                                          Expression.Assign(
                                                              found,
                                                              Expression.Constant(true)
                                                          ),
        // break;
                                                          Expression.Break(breakInner)
        // }
                                                      )
                                                  )
        // }
                                              ),
        // d++;
                                              Expression.PostIncrementAssign(d)
        // }
                                          ),
                                          breakInner
                                      )
                                  ),
        // if
                                  Expression.IfThen(
        // (!found)
                                      Expression.Not(found),
        //    res.Add(n);
                                      Expression.Call(
                                          res,
                                          typeof(List<int>).GetMethod("Add"),
                                          n
                                      )
                                  )
                              ),
        // n++;
                              Expression.PostIncrementAssign(n)
        // }
                          ),
                          breakOuter
                      )
                  ),
                  res
              ),
              to
        // }
          ).Compile();

      Stopwatch sw = Stopwatch.StartNew();

      for (int i = 0; i < 1000; i++)
      {
        Bar = func1(i);
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);

      sw.Restart();

      for (int i = 0; i < 1000; i++)
      {
        Bar = func(i);
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);


    }

    static void Benchmark()
    {

      var mapper = new DefaultMemberMapper();

      var map = mapper.CreateMap(typeof(ComplexSourceType), typeof(ComplexDestinationType)).FinalizeMap();

      var source = new ComplexSourceType
      {
        ID = 5,
        Complex = new NestedSourceType
        {
          ID = 10,
          Name = "test"
        }
      };

      Func<ComplexSourceType, ComplexDestinationType, ComplexDestinationType> f = (src, dest) =>
      {
        dest.ID = src.ID;
        if (src.Complex != null)
        {
          var complexSource = src.Complex;
          var complexDestination = new NestedDestinationType();
          complexDestination.ID = complexSource.ID;
          complexDestination.Name = complexSource.Name;
          dest.Complex = complexDestination;
        }
        return dest;
      };

      var sw = Stopwatch.StartNew();

      for (int i = 0; i < 1000000; i++)
      {
        Foo = new ComplexDestinationType();

        Foo.ID = source.ID;

        if(source.Complex != null)
        {
          Foo.Complex = new NestedDestinationType
          {
            ID = source.Complex.ID + i,
            Name = source.Complex.Name
          };
        }
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);

      sw.Restart();

      for (int i = 0; i < 1000000; i++)
      {
        Foo = f(source, new ComplexDestinationType());
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);

      sw.Restart();

      for (int i = 0; i < 1000000; i++)
      {
        Foo = mapper.Map<ComplexSourceType, ComplexDestinationType>(source);
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);

      var func = (Func<ComplexSourceType, ComplexDestinationType, ComplexDestinationType>)map.MappingFunction;

      var destination = new ComplexDestinationType();

      sw.Restart();

      for (int i = 0; i < 1000000; i++)
      {
        Foo = func(source, new ComplexDestinationType());
      }

      sw.Stop();

      Console.WriteLine(sw.Elapsed);
    }
  }
}
