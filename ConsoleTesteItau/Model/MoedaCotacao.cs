using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTesteItau.Model
{
    class MoedaCotacao
    {
        public string IdMoeda { get; set; }
        public DateTime Data { get; set; }
        public decimal Cotacao { get; set; }
    }
}
