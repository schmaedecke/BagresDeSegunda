using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagresDeSegunda.Models
{
    public class Jogador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
        public int GolsMarcados { get; set; }
        public virtual ICollection<Atuacao> Atuacoes { get; set; }
    }
}
