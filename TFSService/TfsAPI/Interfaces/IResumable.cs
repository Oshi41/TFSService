using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.Interfaces
{
    public interface IResumable
    {
        void Start();

        void Pause();
    }
}
