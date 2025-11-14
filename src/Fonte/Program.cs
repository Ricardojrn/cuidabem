using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

public class Medicamento
{
    public string Nome { get; set; }
    public TimeSpan Horario { get; set; }
    public int QuantidadeComprimidos { get; set; }

    public override string ToString()
    {
        return $"**{Nome}** às {Horario:hh\\:mm} ({QuantidadeComprimidos} comprimido(s))";
    }
}

public class GerenciadorDados
{

    private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "medicamentos.json");

    public List<Medicamento> CarregarMedicamentos()
    {
        if (!File.Exists(NomeArquivo))
        {
            return new List<Medicamento>();
        }

        try
        {
            string jsonString = File.ReadAllText(NomeArquivo);
            return JsonSerializer.Deserialize<List<Medicamento>>(jsonString) ?? new List<Medicamento>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
            return new List<Medicamento>();
        }
    }

    public void SalvarMedicamentos(List<Medicamento> medicamentos)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(medicamentos, options);
            File.WriteAllText(NomeArquivo, jsonString);
            Console.WriteLine("Dados salvos com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar dados: {ex.Message}");
        }
    }
}

public class Program
{
    private static GerenciadorDados _gerenciador = new GerenciadorDados();
    private static List<Medicamento> _medicamentos;

    public static void Main(string[] args)
    {
        Console.WriteLine("--- Controle de Medicamentos ---");

        _medicamentos = _gerenciador.CarregarMedicamentos();
        Console.WriteLine($" {_medicamentos.Count} medicamento(s) carregado(s).");

        ColetarDadosDoUsuario();

        _gerenciador.SalvarMedicamentos(_medicamentos);

        Console.WriteLine("\n--- Monitoramento de Horários ---");
        IniciarMonitoramento();
    }

    private static void ColetarDadosDoUsuario()
    {
        Console.WriteLine("\n--- Cadastro de Novo Medicamento ---");

        Console.Write("Nome do medicamento: ");
        string nome = Console.ReadLine();

        TimeSpan horario;
        while (true)
        {
            Console.Write("Horário (formato HH:mm): ");
            string entrada = Console.ReadLine();

            if (TimeSpan.TryParseExact(entrada, "hh\\:mm", null, out horario))
            {
                break;
            }
            Console.WriteLine("Horário inválido. Use o formato HH:mm (ex: 07:30).");
        }

        int quantidade;
        while (true)
        {
            Console.Write("Quantidade de comprimidos: ");
            if (int.TryParse(Console.ReadLine(), out quantidade) && quantidade > 0)
            {
                break;
            }
            Console.WriteLine("⚠️ Quantidade inválida. Deve ser um número inteiro positivo.");
        }

        var novoMedicamento = new Medicamento
        {
            Nome = nome,
            Horario = horario,
            QuantidadeComprimidos = quantidade
        };

        _medicamentos.Add(novoMedicamento);
        Console.WriteLine($" Medicamento '{nome}' agendado para {horario:hh\\:mm}.");
    }

    private static void IniciarMonitoramento()
    {
        Console.WriteLine("Pressione ESC para sair do monitoramento.");

        while (true)
        {

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Saindo do monitoramento...");
                    break;
                }
            }

            var agora = DateTime.Now;
            var medicamentosParaTomar = _medicamentos
                .Where(m => m.Horario.Hours == agora.Hour && m.Horario.Minutes == agora.Minute)
                .ToList();

            if (medicamentosParaTomar.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n HORA DO MEDICAMENTO! ({agora:HH\\:mm}) 🔔");
                Console.ResetColor();

                foreach (var med in medicamentosParaTomar)
                {
                    Console.WriteLine($">>> Tome seu medicamento: {med}");
                }

                Thread.Sleep(61000);
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }
}

