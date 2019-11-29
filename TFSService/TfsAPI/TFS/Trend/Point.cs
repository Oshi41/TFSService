using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS.Trend
{
    /// <summary>
    /// Точка на графике
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Время для указанной точки
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public double Value { get; set; }
    }
}
