using System.Collections.Generic;

namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 模糊查询
    /// </summary>
    public class Keyword
    {
        /// <summary>
        /// 多条件关系
        /// </summary>
        public MultipleMark MultipleMark { get; set; }

        /// <summary>
        /// 关键字列表
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }
    }
}
