using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class Settings
    {
        /// <summary>
        /// Последний день мониторинга
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// Строка подключения к серверу
        /// </summary>
        public string TfsServer { get; set; }

        /// <summary>
        /// Таск, который мейчас в работе
        /// </summary>
        public string ObservableTaskId { get; set; }
    }
}
