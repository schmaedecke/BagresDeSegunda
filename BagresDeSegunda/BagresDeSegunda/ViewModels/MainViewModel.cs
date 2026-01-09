using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
        public ICommand ExcluirJogadorCommand => new RelayCommand<object>((obj) =>
        {
            if (obj is Jogador jogadorParaRemover)
            {
                var resultado = MessageBox.Show($"Deseja realmente excluir {jogadorParaRemover.Nome}?",
                                      "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if(resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        using(var db = new AppDbContext())
                        {
                            db.Jogadores.Remove(jogadorParaRemover);
                            db.SaveChanges();
                            JogadoresDisponiveis.Remove(jogadorParaRemover);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Não foi possível excluir o jogador. Verifique se ele já possui partidas registradas.\nErro: " + ex.Message);
                    }
                }
            }
        });

        public ICommand AdicionarJogadorCommand => new RelayCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(NomeNovoJogador))
            {
                using (var db = new AppDbContext())
                {
                    var novoJogador = new Jogador { Nome = NomeNovoJogador };
                    db.Jogadores.Add(novoJogador);
                    db.SaveChanges();
                    JogadoresDisponiveis.Add(novoJogador);
                    NomeNovoJogador = string.Empty;
                }
            }
        });

        public MainViewModel()
        {
            JogadoresDisponiveis = new ObservableCollection<Jogador>();
            RankingGeral = new ObservableCollection<Jogador>();
            HistoricoPartidas = new ObservableCollection<Partida>();

            EscalarTime1Command = new RelayCommand<Jogador>(jogador =>
            {
                if (!Time1Escalado.Any(a => a.JogadorId == jogador.Id) && !Time2Escalado.Any(a => a.JogadorId == jogador.Id))
                {
                    Time1Escalado.Add(new Atuacao { Jogador = jogador, JogadorId = jogador.Id, NumeroTime = 1 });
                    JogadoresDisponiveis.Remove(jogador);
                }
            });

            EscalarTime2Command = new RelayCommand<Jogador>(jogador =>
            {
                if (jogador != null && !Time1Escalado.Any(a => a.JogadorId == jogador.Id) && !Time2Escalado.Any(a => a.JogadorId == jogador.Id))
                {
                    Time2Escalado.Add(new Atuacao { Jogador = jogador, JogadorId = jogador.Id, NumeroTime = 2 });
                    JogadoresDisponiveis.Remove(jogador);
                    OnPropertyChanged(nameof(TotalGolsT2));
                }
            });
            SalvarJogoCommand = new RelayCommand(() =>
            {
                SalvarPartida();
            });
            BuscarJogadoresBanco();

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
                    GolsTime1 = TotalGolsT1,
                    GolsTime2 = TotalGolsT2,
                    Atuacoes = new List<Atuacao>()
                };
                int golsT1 = TotalGolsT1;
                int golsT2 = TotalGolsT2;

                foreach (var atuacaoTela in Time1Escalado.Concat(Time2Escalado))
                {
                    var jogadorNoDb = db.Jogadores.Find(atuacaoTela.JogadorId);
                    if (jogadorNoDb != null)
                    {
                        jogadorNoDb.GolsMarcados += atuacaoTela.Gols;
                        if (golsT1 > golsT2 && atuacaoTela.NumeroTime == 1 ||
                            golsT2 > golsT1 && atuacaoTela.NumeroTime == 2)
                        {
                            jogadorNoDb.Vitorias += 1;
                        }
                        else if (golsT1 == golsT2)
                        {
                            jogadorNoDb.Empates += 1;
                        }
                        else
                        {
                            jogadorNoDb.Derrotas += 1;
                        }
                        novaPartida.Atuacoes.Add(new Atuacao
                        {
                            JogadorId = jogadorNoDb.Id,
                            Gols = atuacaoTela.Gols,
                            NumeroTime = atuacaoTela.NumeroTime
                        });
                    }
                }
                    db.Partidas.Add(novaPartida);
                    db.SaveChanges();

                    Time1Escalado.Clear();
                    Time2Escalado.Clear();

                    BuscarJogadoresBanco();
                    AtualizarRankingEHistorico();
            }
        }

        private void AtualizarRankingEHistorico()
        {
            using (var db = new AppDbContext())
            {
                var todosJogadores = db.Jogadores.Include(j => j.Atuacoes).ThenInclude(a => a.Partida).ToList();

                RankingGeral.Clear();

                foreach (var jogador in todosJogadores)
                {
                    int vitorias = 0;
                    int empates = 0;
                    int derrotas = 0;

                    foreach (var atuacao in jogador.Atuacoes)
                    {
                        var partida = atuacao.Partida;
                        int golsDoTime = atuacao.Gols;
                        int golsDoAdversario = partida.Atuacoes
                            .Where(a => a.NumeroTime != atuacao.NumeroTime)
                            .Sum(a => a.Gols);
                        if (golsDoTime > golsDoAdversario)
                            vitorias++;
                        else if (golsDoTime == golsDoAdversario)
                            empates++;
                        else
                            derrotas++;
                    }

                    jogador.Vitorias = vitorias;
                    jogador.Empates = empates;
                    jogador.Derrotas = derrotas;

                    RankingGeral.Add(jogador);

                }

                var listaOrdenada = RankingGeral
                    .OrderByDescending(j => j.Vitorias)
                    .ThenBy(j => j.GolsMarcados)
                    .ToList();

                HistoricoPartidas.Clear();
                var jogos = db.Partidas.OrderByDescending(p => p.Data).Take(10).ToList();
                foreach (var p in jogos) HistoricoPartidas.Add(p);
            }
        }

        private void BuscarJogadoresBanco()
        {
            try
            {
                using ( var db = new AppDbContext())
                {
                    var listaBanco = db.Jogadores.ToList();
                    JogadoresDisponiveis.Clear();
                    foreach(var jogador in listaBanco)
                    {
                        JogadoresDisponiveis.Add(jogador);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar jogadores do MySQL: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
