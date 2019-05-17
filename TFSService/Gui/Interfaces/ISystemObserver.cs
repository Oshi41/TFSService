using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui.Interfaces
{
    public interface ISystemObserver
    {
        /// <summary>
        /// пользователь зашёл в систему
        /// </summary>
        event EventHandler Login;

        /// <summary>
        /// Пользователь вышел из системы
        /// </summary>
        event EventHandler Logogff;

        /// <summary>
        /// Необходимо списать опр. кол-во часов
        /// </summary>
        event EventHandler<int> WriteHours;
    }
}
