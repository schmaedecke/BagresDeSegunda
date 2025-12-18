using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagresDeSegunda.Models
{
    public class Atuacao
    {
        public int Id { get; set; } 

        public int PartidaId { get; set; }
        public virtual Partida Partida { get; set; }

        public int JogadorId { get; set; }
        public virtual Jogador Jogador { get; set; }

        public int NumeroTime { get; set; }
        public int Gols { get; set; }
    }
}
