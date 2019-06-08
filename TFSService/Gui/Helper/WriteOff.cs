using System;
using Newtonsoft.Json;

namespace Gui.Helper
{
    public class WriteOff
    {
        /// <summary>
        ///     Записанный программой чекин
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hours"></param>
        /// <param name="time"></param>
        /// <param name="createdByUser"></param>
        /// <param name="recorded"></param>
        [JsonConstructor]
        public WriteOff(int id, int hours, DateTime time, bool createdByUser, bool recorded)
        {
            Id = id;
            Hours = hours;
            Time = time;
            CreatedByUser = createdByUser;
            Recorded = recorded;
        }

        /// <summary>
        ///     Записанное программой время
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hours"></param>
        public WriteOff(int id, int hours)
            : this(id, hours, DateTime.Now, false, false)
        {
        }

        /// <summary>
        ///     Созданный пользователем чекин
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hours"></param>
        /// <param name="time"></param>
        public WriteOff(int id, int hours, DateTime time)
            : this(id, hours, time, true, true)
        {
        }

        public void Increase(int hours)
        {
            Hours += hours;
        }

        #region Props

        /// <summary>
        ///     ID раочего элемента
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Кол-во списанных часов
        /// </summary>
        public int Hours { get; private set; }

        /// <summary>
        ///     Время списания часа
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        ///     Чекин от пользователя?
        /// </summary>
        public bool CreatedByUser { get; }

        /// <summary>
        ///     Была ли запись внесена в TFS
        /// </summary>
        public bool Recorded { get; }

        #endregion
    }
}