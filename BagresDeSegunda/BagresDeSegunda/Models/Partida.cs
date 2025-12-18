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
        public int GolsTimeA { get; set; }
        public int GolsTimeB { get; set; }
        public virtual ICollection<Atuacao> Atuacoes { get; set; }
    }
}
