using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Gui.Helper
{
    /// <summary>
    /// Выполняет только одну операцию в определенный промежуток времени,
    /// что позволяет избежать циклических вызовов. Имеет возможность блокировки выполнения пользователем
    /// </summary>
    public class ActionArbiter
    {
        private int _skip;
        private bool _block;
        private bool _isExecuting;

        /// <summary>
        ///     Выполняем операцию
        /// </summary>
        /// <param name="action"></param>
        public void Do(Action action)
        {
            try
            {
                if (!IsFree())
                    return;

                Capture();
                action();
            }
            catch (Exception e)
            {
                Trace.Write(e);
                throw;
            }
            finally
            {
                Release();
            }
        }

        /// <summary>
        /// Пропускаю кол-во вызовов
        /// </summary>
        /// <param name="i"></param>
        public void Skip(int i)
        {
            _skip += i;
            _skip = Math.Max(0, _skip);
        }

        /// <summary>
        ///     Выставляем блокирование для операции
        /// </summary>
        /// <param name="isBlock"></param>
        public virtual void SetBlock(bool isBlock)
        {
            _block = isBlock;
        }

        /// <summary>
        ///     Может ли арбитр выполнить действие
        /// </summary>
        /// <returns></returns>
        public bool IsFree()
        {
            if(_isExecuting || _block)
            {
                return false;
            }

            // Пропускаю первые вхождения
            if (_skip > 0)
            {
                _skip--;
                return false;
            }

            return true;
        }


        protected void Capture()
        {
            _isExecuting = true;
        }

        protected void Release()
        {
            _isExecuting = false;
        }
    }

    /// <summary>
    /// То же, что и супер класс, но с поддержкой async/await
    /// </summary>    
    public class ActionArbiterAsync : ActionArbiter
    {
        private CancellationToken _token;

        public async Task DoAsync(Action action)
        {
            try
            {
                if (!IsFree())
                    return;

                Capture();

                await Task.Factory.StartNew(action, _token = new CancellationToken());
            }
            catch (Exception e)
            {
                Trace.Write(e);
                throw;
            }
            finally
            {
                Release();
            }
        }

        public override void SetBlock(bool isBlock)
        {
            base.SetBlock(isBlock);

            if (isBlock) _token.ThrowIfCancellationRequested();
        }
    }
}