using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS.Trend
{
    /// <summary>
    /// Представление графика трудосгораний
    /// </summary>
    public class Chart
    {
        /// <summary>
        /// Трудосгорание человека
        /// </summary>
        public IList<Point> Items { get; set; } = new List<Point>();

        /// <summary>
        /// Доступное трудосгорание
        /// </summary>
        public IList<Point> Available { get; set; } = new List<Point>();
    }
}
