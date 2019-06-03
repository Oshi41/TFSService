using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gui.Helper
{
    public static class WpfUtil
    {
        /// <summary>
        /// Ищет потомка по имени
        /// </summary>
        /// <typeparam name="T">Тип контрола</typeparam>
        /// <param name="parent">Родитель</param>
        /// <param name="childName">Имя контрола</param>
        /// <param name="maxDepth">Макс. кол-во поколений, по которым ищем</param>
        /// <returns>Ищет по поколениям, а не по ветвям</returns>
        public static T FindChildByName<T>(DependencyObject parent, string childName, int maxDepth = int.MaxValue)
            where T : FrameworkElement
        {
            return FindChildByName<T>(childName, maxDepth, parent);
        }

        /// <summary>
        /// Ищет потомка по имени
        /// </summary>
        /// <typeparam name="T">Тип потомка</typeparam>
        /// <param name="childName">Имя контрола</param>
        /// <param name="maxSteps">Максимальное кол-во шагов вглубь</param>
        /// <param name="parents">Поколение родителей</param>
        /// <returns>Ищет потомки по поколениям</returns>
        private static T FindChildByName<T>(string childName, int maxSteps, params DependencyObject[] parents)
            where T : FrameworkElement
        {
            // Выхожу если некорректное имя, либо превысил кол-во шагов вглубь
            if (!parents.Any()
                || string.IsNullOrEmpty(childName)
                || maxSteps <= 0)
            {
                return null;
            }

            // обходим список непосредственных потомков
            List<DependencyObject> directChilds = new List<DependencyObject>();
            foreach (var parent in parents)
            {
                for (int i = 0, end = VisualTreeHelper.GetChildrenCount(parent); i < end; i++)
                {
                    //  добавили контрол в список непосредственных потомков
                    var child = VisualTreeHelper.GetChild(parent, i);
                    directChilds.Add(child);

                    // если имя и тип совпадает, возвращаем
                    if (child is T result && result.Name == childName)
                    {
                        return result;
                    }
                }
            }

            // Спускаюсь вниз на шаг, поэтому его вычитаю
            return FindChildByName<T>(childName, maxSteps - 1, directChilds.ToArray());
        }
    }
}
