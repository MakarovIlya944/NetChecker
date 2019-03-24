using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    public interface IMatrix
    {
        IMatrix ReverseMatrix();

        
    }
}
