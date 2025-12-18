using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BagresDeSegunda.Data;
using BagresDeSegunda.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace BagresDeSegunda.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Jogador> JogadoresDisponiveis { get; set; }
        public ObservableCollection<Jogador> RankingGeral { get; set; }
        public ObservableCollection<Partida> HistoricoPartidas { get; set; }
        public ObservableCollection<Atuacao> Time1Escalado { get; set; } = new ObservableCollection<Atuacao>();
        public ObservableCollection<Atuacao> Time2Escalado { get; set; } = new ObservableCollection<Atuacao>();

        private string _NomeNovoJogador;
        public string NomeNovoJogador
        {
            get => _NomeNovoJogador;
            set
            {
                _NomeNovoJogador = value; OnPropertyChanged();
            }
        }

        public ICommand EscalarTime1Command { get; }
        public ICommand EscalarTime2Command { get; }
        public ICommand SalvarJogoCommand { get; }

        public MainViewModel()
        {
            JogadoresDisponiveis = new ObservableCollection<Jogador>();
            RankingGeral = new ObservableCollection<Jogador>();
            HistoricoPartidas = new ObservableCollection<Partida>();

            EscalarTime1Command = new RelayCommand<Jogador>(jogador =>
            {
                if (jogador != null)
                {
                    Time1Escalado.Add(new Atuacao { Jogador = jogador, NumeroTime = 1 });
                    JogadoresDisponiveis.Remove(jogador);
                }
            });
            SalvarJogoCommand = new RelayCommand (() =>
            {
                SalvarPartida();
            });
        }

        public int TotalGolsT1 => Time1Escalado.Sum(a => a.Gols);
        public int TotalGolsT2 => Time2Escalado.Sum(a => a.Gols);

        private void SalvarPartida()
        {
            using (var db = new AppDbContext())
            {
                var novaPartida = new Partida
                {
                    Data = DateTime.Now,
                    GolsTimeA = TotalGolsT1,
                    GolsTimeB = TotalGolsT2,
                    Atuacoes = new List<Atuacao>()
                };

                // Adiciona as atuações do Time 1 e Time 2
                foreach (var a in Time1Escalado.Concat(Time2Escalado))
                {
                    // Vinculamos apenas o ID para o banco não tentar duplicar o Jogador
                    novaPartida.Atuacoes.Add(new Atuacao
                    {
                        JogadorId = a.JogadorId,
                        Gols = a.Gols,
                        NumeroTime = a.NumeroTime
                    });
                }

                db.Partidas.Add(novaPartida);
                db.SaveChanges();

                // Limpa a tela para o próximo jogo
                Time1Escalado.Clear();
                Time2Escalado.Clear();

                // Atualiza as listas da tela
                AtualizarRankingEHistorico();
            }
        }

        private void AtualizarRankingEHistorico()
        {
            using (var db = new AppDbContext())
            {
                var todosJogadores = db.Jogadores.Include(j => j.Atuacoes).ThenInclude(a => a.Partida).ToList();

                
                // Aqui você calcula Vitórias, Empates e Derrotas baseado nas atuações
                // e atualiza a ObservableCollection 'RankingGeral'

                HistoricoPartidas.Clear();
                var jogos = db.Partidas.OrderByDescending(p => p.Data).Take(10).ToList();
                foreach (var p in jogos) HistoricoPartidas.Add(p);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
