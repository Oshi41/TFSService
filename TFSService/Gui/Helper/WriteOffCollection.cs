using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsAPI.Extentions;
using TfsAPI.Interfaces;

namespace Gui.Helper
{
    public class WriteOffCollection : ObservableCollection<WriteOff>
    {
        #region Public methods

        /// <summary>
        ///     Прошел час штатной работы программы
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hours"></param>
        public void ScheduleWork(int id, int hours)
        {
            var item = new WriteOff(id, hours);
            Add(item);

            Trace.WriteLine(
                $"{nameof(WriteOffCollection)}.{nameof(ScheduleWork)}: Hour scheduled at {item.Time.ToShortTimeString()}");
        }

        /// <summary>
        ///     Проверяем, записали ли чекины от пользователя
        /// </summary>
        /// <param name="tfs"></param>
        public void SyncCheckins(ITfsApi tfs)
        {
            var checkins = tfs.GetWriteoffs(DateTime.Today, DateTime.Now);

            Trace.WriteLine($"{nameof(WriteOffCollection)}.{nameof(SyncCheckins)}: Founded {checkins.Count} changes");

            foreach (var checkin in checkins)
            {
                var id = checkin.Key.WorkItem.Id;
                var date = (DateTime) checkin.Key.Fields[CoreField.ChangedDate].Value;

                if (!this.Any(x => x.Time == date && x.Id == id))
                {
                    var userCheckIn = new WriteOff(id, checkin.Value, date);
                    Add(userCheckIn);

                    Trace.WriteLine($"{nameof(WriteOffCollection)}.{nameof(SyncCheckins)}: Detected new check-in, " +
                                    $"Id - {checkin.Key.WorkItem.Id}, Time - {date.ToShortTimeString()}");
                }
            }
        }

        /// <summary>
        ///     Сколько часов программа поставила в очередь
        /// </summary>
        /// <returns></returns>
        public int ScheduledTime()
        {
            return GetManual(this).Sum(x => x.Hours);
        }

        /// <summary>
        ///     Сколько сегодня было зачекинено пользователем
        /// </summary>
        /// <returns></returns>
        public int CheckinedTime()
        {
            return this.Where(x => x.Recorded && x.CreatedByUser).Sum(x => x.Hours);
        }

        /// <summary>
        ///     Очищаем предыдущие записи
        /// </summary>
        public void ClearPrevRecords()
        {
            RemoveAll(x => !x.Time.IsToday());
        }

        /// <summary>
        ///     Списываем запланированную работу
        /// </summary>
        /// <param name="api">TFS API</param>
        /// <param name="capacity">Кол-во рабочих часов в этом дне</param>
        public void CheckinScheduledWork(ITfsApi api, int capacity)
        {
            // Обновили историю чекинов
            SyncCheckins(api);

            // Обрезали, если вышли за предел кол-ва часов
            CutOffByCapacity(capacity);

            CheckinWork(api);
        }

        /// <summary>
        ///     Синхронизируем дневной плн списания времени. Кол-во списанного времени должно быть равно дневной норме.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="capacity"></param>
        /// <param name="currentItem"></param>
        public void SyncDailyPlan(ITfsApi api, int capacity, Func<WorkItem> currentItem)
        {
            // Обновили историю чекинов
            SyncCheckins(api);

            // Обрезали, если вышли за предел кол-ва часов
            CutOffByCapacity(capacity);

            // сколько было списно пользователем
            var byUser = CheckinedTime();
            // сколько было списано
            var scheduled = ScheduledTime();

            var delta = capacity - byUser - scheduled;

            // Нужно распланировать ещё времени
            if (delta > 0)
            {
                var item = currentItem();

                ScheduleWork(item.Id, delta);
            }

            CheckinWork(api);
        }

        #endregion

        #region Constructors

        public WriteOffCollection(IEnumerable<WriteOff> source)
            : base(source)
        {
        }

        public WriteOffCollection()
        {
        }

        #endregion

        #region Private

        /// <summary>
        ///     Записываем всю работу в TFS.
        ///     В случаем с чекином вчерашней работы, она записывается отдельно и не мешает
        ///     дневному кол-ву работы
        /// </summary>
        private void CheckinWork(ITfsApi tfs)
        {
            var manual = Merge(GetManual(this));

            foreach (var item in manual)
                try
                {
                    var revision = tfs.WriteHours(tfs.FindById(item.Id), (byte) item.Hours, true);

                    RemoveAll(x => x.Id == item.Id);

                    if (revision != null)
                    {
                        var time = (DateTime) revision.Fields[CoreField.ChangedDate].Value;

                        Add(new WriteOff(revision.WorkItem.Id,
                            item.Hours,
                            time,
                            // Если запись была запланирована сегодня, считаем это обычным
                            // чекином юзера
                            item.Time.IsToday(),
                            true));
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }

            ClearPrevRecords();
        }

        /// <summary>
        ///     Обрезаю запланированную работу по чекинам пользователя
        ///     или по его дневному графику
        /// </summary>
        /// <param name="maxHoursPerDay"></param>
        private void CutOffByCapacity(int maxHoursPerDay)
        {
            // Часы, которая программа поставила на ожидание
            var manual = GetManual(this);

            // Сколько пользователь начекинил
            var alreadyRecorded = CheckinedTime();
            // сколько программа поставила в очередь
            var scheduled = ScheduledTime();

            // Поставили в очередь больше, чем планировали, вырезаем
            while (scheduled > maxHoursPerDay && manual.Any())
            {
                var item = manual.First();

                Remove(item);
                manual.Remove(item);

                scheduled -= item.Hours;
            }

            // Ничего не чекинил, выходим
            if (alreadyRecorded < 1)
            {
                Trace.WriteLine(
                    $"{nameof(WorkItemCollection)}.{nameof(CutOffByCapacity)}: User is not write off work hours today");
                return;
            }

            Trace.WriteLine(
                $"{nameof(WorkItemCollection)}.{nameof(CutOffByCapacity)}: User wrote off {alreadyRecorded} " +
                $"hour(s), capacity is {maxHoursPerDay}");

            // Пользователь уже начекинил на рабочее время, 
            // вырезаем всё.
            if (alreadyRecorded >= maxHoursPerDay)
            {
                RemoveRange(manual);
                return;
            }

            // Вырезаем часы, поставленные в очередь
            // программой, начиная с самых новых
            while (alreadyRecorded > 0 && manual.Any())
            {
                Remove(manual[0]);
                manual.RemoveAt(0);

                alreadyRecorded--;
            }


            Trace.WriteLine(
                $"{nameof(WorkItemCollection)}.{nameof(CutOffByCapacity)}: Scheduled {manual.Count} records:");
        }

        private void RemoveAll(Func<WriteOff, bool> condition)
        {
            var toRemove = this.Where(condition).ToList();
            foreach (var item in toRemove) Remove(item);
        }

        private void RemoveRange(IList<WriteOff> source)
        {
            foreach (var item in source) Remove(item);
        }

        #endregion

        #region static

        /// <summary>
        ///     Мерджит чекины пользователя и запланированные программой
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static List<WriteOff> Merge(IList<WriteOff> source)
        {
            var result = new List<WriteOff>();

            void MergeByCondition(Func<WriteOff, bool> func)
            {
                foreach (var off in source.Where(func))
                {
                    var first = result.FirstOrDefault(x => x.Id == off.Id
                                                           && x.CreatedByUser == off.CreatedByUser
                                                           && x.Recorded == off.Recorded);

                    if (first != null)
                        first.Increase(off.Hours);
                    else
                        result.Add(off);
                }
            }

            MergeByCondition(x => x.CreatedByUser && x.Recorded);
            MergeByCondition(x => !x.CreatedByUser && !x.Recorded);


            return result;
        }

        /// <summary>
        ///     Возвращает список запланированных программой чекинов. Отсортированы от свежих к старым
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static List<WriteOff> GetManual(IList<WriteOff> source)
        {
            return source.Where(x => !x.Recorded && !x.CreatedByUser)
                .OrderByDescending(x => x.Time)
                .ToList();
        }

        #endregion
    }
}