using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Gui.Helper
{
    public class ActionArbiter
    {
        private bool _isExecuting;
        private bool _block;

        /// <summary>
        /// Выполняем операцию
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
        /// Выставляем блокирование для операции
        /// </summary>
        /// <param name="isBlock"></param>
        public virtual void SetBlock(bool isBlock)
        {
            _block = isBlock;
        }

        /// <summary>
        /// Может ли арбитр выполнить действие
        /// </summary>
        /// <returns></returns>
        public bool IsFree()
        {
            return !_isExecuting && !_block;
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

                await Task.Factory.StartNew(action, (_token = new CancellationToken()));
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

            if (isBlock)
            {
                _token.ThrowIfCancellationRequested();
            }
        }
    }
}
