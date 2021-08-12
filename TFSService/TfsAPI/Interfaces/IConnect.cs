using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface IConnect
    {
        /// <summary>
        /// Текущее подключение к TFS
        /// </summary>
        TfsTeamProjectCollection Tfs { get; set; }

        /// <summary>
        /// Сервис для работы с рабочими жлементами
        /// </summary>
        WorkItemStore WorkItemStore { get; }

        /// <summary>
        /// Сервис для линковки рабочих элементов
        /// </summary>
        ILinking Linking { get; }

        /// <summary>
        /// Серсия для работы с чекинами
        /// </summary>
        VersionControlServer VersionControlServer { get; }

        /// <summary>
        /// Сервис для работы с пользователями TFS
        /// </summary>
        IIdentityManagementService2 ManagementService2 { get; }

        /// <summary>
        /// Сервис для работы с командами TFS
        /// </summary>
        TfsTeamService TeamService { get; }

        /// <summary>
        /// Текущий проект
        /// </summary>
        Project Project { get; set; }

        /// <summary>
        /// От чьего имени действуем
        /// </summary>
        string Name { get; set; }

        event EventHandler<Project> ProjectChanged;
        event EventHandler<TfsTeamProjectCollection> TfsChanged;

        /// <summary>
        /// Подключаемся к TSF
        /// </summary>
        /// <param name="url">Адрес подключения</param>
        /// <param name="projectName">Опуиональное имя проекта</param>
        /// <returns>Статус подключения</returns>
        /// <exception cref="ArgumentException"></exception>
        void Connect(string url, string projectName = null);
    }
}