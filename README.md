# EF.Core.Expansion.Dynamic[初步完成,待测试覆盖]
基于EF的扩展,试图参照elasticsearch的查询表达式(Query DSL),由前端控制整个查询过程

支持操作符:[>,<,>=,<=,contains,!contains,in,!in,startswith,!startswith,endswith,!endswith,]

如:
```
    class Entity
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        public int Age { get; set; }

        public int Age2 { get; set; }
    }

    class Db2 : DbContext
    {
        public DbSet<Entity> Entity { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            PageQueryParameter pageQueryParameter = new PageQueryParameter()
            {
                Condition = new QueryCondition()
                {
                    Keyword = new Keyword()
                    {
                        MultipleMark = MultipleMark.Or,
                        Keywords = new[] { "a", "b" },
                    },
                    Filter = new Filter()
                    {
                        MultipleMark = MultipleMark.And,
                        CompareConditions = new[] {
                              new CompareCondition()
                        {
                            Compare="==",
                            Name="age",
                            Value="age2",
                        },
                        new CompareCondition()
                        {
                            Compare="==",
                            Name="name",
                            Value="nam2",
                        },
                        new CompareCondition()
                        {
                            Compare="!=",
                            Name="name",
                            Value="nam2",
                        },
                        new CompareCondition()
                        {
                            Compare="contains",
                            Name="name",
                            Value="nam2",
                        },
                        new CompareCondition()
                        {
                            Compare="!contains",
                            Name="name",
                            Value="nam2",
                        },
                    },
                        MuchConditions = new[]
                    {
                        new MatchCondition()
                        {
                            Name="name",
                            Compare="in",
                            Values=new string[]
                            {
                                "1","2","3"
                            }
                        },
                        new MatchCondition()
                        {
                            Name="name",
                            Compare="!in",
                            Values=new string[]
                            {
                                "1","2","3"
                            }
                        },
                           new MatchCondition()
                        {
                            Name="age",
                            Compare="in",
                            Values=new string[]
                            {
                                "1","2","3"
                            }
                        },
                    },
                        Filters = new[]
                    {
                        new Filter()
                        {
                            MultipleMark=MultipleMark.Or,
                            MuchConditions=new []
                            {
                                new MatchCondition()
                                {
                                    Compare="in",
                                    Name="age",
                                    Values=new string[]
                                    {
                                        "1","4"
                                    }
                                },
                                  new MatchCondition()
                                {
                                    Compare="in",
                                    Name="age",
                                    Values=new string[]
                                    {
                                        "1","4","9999"
                                    }
                                }
                            }
                        },
                        new Filter()
                        {
                            MultipleMark=MultipleMark.Or,
                            MuchConditions=new []
                            {
                                new MatchCondition()
                                {
                                    Compare="in",
                                    Name="age",
                                    Values=new string[]
                                    {
                                        "1","4"
                                    }
                                }
                            }
                        }
                    }
                    }
                },
                Sortings = new[]
            {
                new SortingParameter()
                {
                    Name="name",
                    SortMark=SortMark.Desc
                },
                new SortingParameter()
                {
                    Name="age",
                    SortMark=SortMark.Dsc
                },
                new SortingParameter()
                {
                    Name="age",
                    SortMark=SortMark.Dsc
                },
            }
            };

            var iq = new Db2().Entity.DynamicQuery(pageQueryParameter.Condition);

            Console.WriteLine(iq.ToString());

            iq = iq.DynamicSort(pageQueryParameter.Sortings);

            Console.WriteLine(iq.ToString());

            var param = Newtonsoft.Json.JsonConvert.SerializeObject(pageQueryParameter);
            Console.WriteLine(param);

            Console.ReadLine();
        }

    }

    class PageQueryParameter : IPageQueryParameter
    {
        public int Total { get; set; }

        public QueryCondition Condition { get; set; }

        public IEnumerable<SortingParameter> Sortings { get; set; }
    }

```

