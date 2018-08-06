# EF.Core.Expansion.Dynamic[坑填中..]
基于EF的扩展,试图参照elasticsearch的查询表达式(Query DSL),由前端控制整个查询过程

如:
```
var iq = new Db().Entity.AssemblyCondition(new Query()
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
                    }
                }
            });

```

