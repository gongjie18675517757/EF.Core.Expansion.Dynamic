# EF.Core.Expansion.Dynamic[初步完成,待测试覆盖]
基于EF的扩展,试图参照elasticsearch的查询表达式(Query DSL),由前端控制整个查询过程

支持操作符:[>,<,>=,<=,contains,!contains,in,!in,startswith,!startswith,endswith,!endswith,]

## 示例代码:
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

## 参数json
```
{
"Total": 0,
"Condition": {
"Keyword": {
    "MultipleMark": 1,
    "Keywords": [
    "a",
    "b"
    ]
},
"Filter": {
    "MultipleMark": 0,
    "Filters": [
        {
        "MultipleMark": 1,
        "Filters": null,
        "CompareConditions": null,
        "MuchConditions": [
            {
                "Name": "age",
                "Compare": "in",
                "Values": [
                "1",
                "4"
                ]
            },
            {
                "Name": "age",
                "Compare": "in",
                "Values": [
                    "1",
                    "4",
                    "9999"
                ]
            }
        ]
        },
        {
            "MultipleMark": 1,
            "Filters": null,
            "CompareConditions": null,
            "MuchConditions": [
                {
                    "Name": "age",
                    "Compare": "in",
                    "Values": [
                        "1",
                        "4"
                    ]
                }
            ]
        }
    ],
    "CompareConditions": [
    {
    "Name": "name",
    "Value": "nam2",
    "Compare": "=="
    },
    {
    "Name": "name",
    "Value": "nam2",
    "Compare": "!="
    },
    {
    "Name": "name",
    "Value": "nam2",
    "Compare": "contains"
    },
    {
    "Name": "name",
    "Value": "nam2",
    "Compare": "!contains"
    }
    ],
    "MuchConditions": [
    {
    "Name": "name",
    "Compare": "in",
    "Values": [
    "1",
    "2",
    "3"
    ]
    },
    {
    "Name": "name",
    "Compare": "!in",
    "Values": [
    "1",
    "2",
    "3"
    ]
    },
    {
    "Name": "age",
    "Compare": "in",
    "Values": [
    "1",
    "2",
    "3"
    ]
    }
    ]
    }
    },
"Sortings": [
{
"Name": "name",
"SortMark": 1
},
{
"Name": "age",
"SortMark": 0
},
{
"Name": "age",
"SortMark": 0
}
]
}
```

## 生成的SQL语句
```
SELECT
    [Extent1].[Id] AS [Id],
    [Extent1].[Name] AS [Name],
    [Extent1].[Age] AS [Age],
    [Extent1].[Age2] AS [Age2]
    FROM [dbo].[Entities] AS [Extent1]
    WHERE ((([Extent1].[Name] IS NOT NULL) AND ([Extent1].[Name] LIKE N'%a%')) OR (([Extent1].[Name] IS NOT NULL) AND ([Extent1].[Name] LIKE N'%b%'))) AND ([Extent1].[Age] = [Extent1].[Age2]) AND (N'nam2' = [Extent1].[Name]) AND ( NOT ((N'nam2' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL))) AND ([Extent1].[Name] IS NOT NULL) AND ([Extent1].[Name] LIKE N'%nam2%') AND (([Extent1].[Name] IS NULL) OR ( NOT ([Extent1].[Name] LIKE N'%nam2%'))) AND ([Extent1].[Name] IN (N'1', N'2', N'3')) AND ([Extent1].[Name] IS NOT NULL) AND ( NOT (([Extent1].[Name] IN (N'1', N'2', N'3')) AND ([Extent1].[Name] IS NOT NULL))) AND ([Extent1].[Age] IN (1, 2, 3)) AND (([Extent1].[Age] IN (1, 4)) OR ([Extent1].[Age] IN (1, 4, 9999))) AND ([Extent1].[Age] IN (1, 4))
    ORDER BY [Extent1].[Name] DESC, [Extent1].[Age] ASC
```
