using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagresDeSegunda.Models
{
    public class Partida
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public int GolsTime1 { get; set; }
        public int GolsTime2 { get; set; }
        public virtual ICollection<Atuacao> Atuacoes { get; set; }
    }
}
