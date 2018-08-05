using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EF.Core.Expansion.Dynamic;
namespace Test
{
    class Entity
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        public int Age { get; set; }
    }

    class Db : DbContext
    {
        public DbSet<Entity> Entity { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var ex = Expand.GetExpression<Entity>(a=>a.Name!="");
            ex=ex.And(x => x.Name == "");
            ex = ex.Or(x => x.Age > 10);

            var xx = ex.Compile();

            xx(new Entity());

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

            Console.WriteLine(iq.ToString());

            //Console.ReadLine();
        }

    }


}
