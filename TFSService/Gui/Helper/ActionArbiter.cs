using System;
using System.Diagnostics;

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
                if (CanExecute())
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
        public void SetBlock(bool isBlock)
        {
            _block = isBlock;
        }



        private void Capture()
        {
            _isExecuting = true;
        }

        private void Release()
        {
            _isExecuting = false;
        }

        private bool CanExecute()
        {
            return !_isExecuting && !_block;
        }
    }
}
