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

       public class Consulta
       {
           public int Id { get; set; }
           public string NomePaciente { get; set; }
           public string Medico { get; set; }
           public DateTime DataHora { get; set; }
           public string Local { get; set; }
           public string Status { get; set; } = "Agendada";

           public override string ToString()
           {
               return $"[{Id}] {NomePaciente} - Dr(a). {Medico} em {DataHora:dd/MM/yyyy HH:mm} - {Local} ({Status})";
           }
       }

       public class GerenciadorConsultas
       {
           private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "consultas.json");

           public List<Consulta> Carregar()
           {
               if (!File.Exists(NomeArquivo))
                   return new List<Consulta>();

               try
               {
                   string json = File.ReadAllText(NomeArquivo);
                   return JsonSerializer.Deserialize<List<Consulta>>(json) ?? new List<Consulta>();
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao carregar consultas: {ex.Message}");
                   return new List<Consulta>();
               }
           }

           public void Salvar(List<Consulta> consultas)
           {
               try
               {
                   var options = new JsonSerializerOptions { WriteIndented = true };
                   string json = JsonSerializer.Serialize(consultas, options);
                   File.WriteAllText(NomeArquivo, json);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao salvar consultas: {ex.Message}");
               }
           }

           public void CadastrarConsulta()
           {
               var consultas = Carregar();

               Console.Write("Nome do paciente: ");
               string paciente = Console.ReadLine();

               Console.Write("Nome do médico: ");
               string medico = Console.ReadLine();

               DateTime dataHora;
               while (true)
               {
                   Console.Write("Data e horário (dd/MM/yyyy HH:mm): ");
                   string entrada = Console.ReadLine();
                   if (DateTime.TryParse(entrada, out dataHora))
                       break;

                   Console.WriteLine("Data/hora inválida. Tente novamente.");
               }

               Console.Write("Local da consulta: ");
               string local = Console.ReadLine();

               int novoId = consultas.Any() ? consultas.Max(c => c.Id) + 1 : 1;

               var consulta = new Consulta
               {
                   Id = novoId,
                   NomePaciente = paciente,
                   Medico = medico,
                   DataHora = dataHora,
                   Local = local
               };

               consultas.Add(consulta);
               Salvar(consultas);

               Console.WriteLine("\n✅ Consulta cadastrada com sucesso!");
               Console.WriteLine(consulta);
           }

           public void ListarConsultas()
           {
               var consultas = Carregar();
               if (!consultas.Any())
               {
                   Console.WriteLine("Não há consultas cadastradas.");
                   return;
               }

               Console.WriteLine("\n--- Consultas ---");
               foreach (var c in consultas.OrderBy(c => c.DataHora))
               {
                   Console.WriteLine(c);
               }
           }

           public void ReagendarConsulta()
           {
               var consultas = Carregar();
               if (!consultas.Any())
               {
                   Console.WriteLine("Não há consultas para reagendar.");
                   return;
               }

               ListarConsultas();
               Console.Write("\nDigite o ID da consulta a reagendar: ");
               if (!int.TryParse(Console.ReadLine(), out int id))
               {
                   Console.WriteLine("ID inválido.");
                   return;
               }

               var consulta = consultas.FirstOrDefault(c => c.Id == id);
               if (consulta == null)
               {
                   Console.WriteLine("Consulta não encontrada.");
                   return;
               }

               DateTime novaData;
               while (true)
               {
                   Console.Write("Nova data e horário (dd/MM/yyyy HH:mm): ");
                   string entrada = Console.ReadLine();
                   if (DateTime.TryParse(entrada, out novaData))
                       break;

                   Console.WriteLine("Data/hora inválida. Tente novamente.");
               }

               consulta.DataHora = novaData;
               consulta.Status = "Reagendada";

               Salvar(consultas);

               Console.WriteLine("\n🔁 Consulta reagendada com sucesso!");
               Console.WriteLine(consulta);
           }
       }

       public class PedidoMedicamento
       {
           public int Id { get; set; }
           public string NomePaciente { get; set; }
           public string Medicamento { get; set; }
           public int Quantidade { get; set; }
           public string EnderecoEntrega { get; set; }
           public string Status { get; set; } = "Pendente";
           public DateTime DataPedido { get; set; } = DateTime.Now;

           public override string ToString()
           {
               return $"[{Id}] {NomePaciente} - {Medicamento} x{Quantidade} - {Status} (pedido em {DataPedido:dd/MM/yyyy HH:mm})";
           }
       }
       public class GerenciadorPedidosMedicamentos
       {
           private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "pedidos_medicamentos.json");

           public List<PedidoMedicamento> Carregar()
           {
               if (!File.Exists(NomeArquivo))
                   return new List<PedidoMedicamento>();

               try
               {
                   string json = File.ReadAllText(NomeArquivo);
                   return JsonSerializer.Deserialize<List<PedidoMedicamento>>(json) ?? new List<PedidoMedicamento>();
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao carregar pedidos: {ex.Message}");
                   return new List<PedidoMedicamento>();
               }
           }

           public void Salvar(List<PedidoMedicamento> pedidos)
           {
               try
               {
                   var options = new JsonSerializerOptions { WriteIndented = true };
                   string json = JsonSerializer.Serialize(pedidos, options);
                   File.WriteAllText(NomeArquivo, json);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao salvar pedidos: {ex.Message}");
               }
           }

           public void CadastrarPedido()
           {
               var pedidos = Carregar();

               Console.Write("Nome do paciente: ");
               string paciente = Console.ReadLine();

               Console.Write("Nome do medicamento: ");
               string medicamento = Console.ReadLine();

               int quantidade;
               while (true)
               {
                   Console.Write("Quantidade: ");
                   if (int.TryParse(Console.ReadLine(), out quantidade) && quantidade > 0)
                       break;
                   Console.WriteLine("Quantidade inválida.");
               }

               Console.Write("Endereço de entrega: ");
               string endereco = Console.ReadLine();

               int novoId = pedidos.Any() ? pedidos.Max(p => p.Id) + 1 : 1;

               var pedido = new PedidoMedicamento
               {
                   Id = novoId,
                   NomePaciente = paciente,
                   Medicamento = medicamento,
                   Quantidade = quantidade,
                   EnderecoEntrega = endereco
               };

               pedidos.Add(pedido);
               Salvar(pedidos);

               Console.WriteLine("\n🛒 Pedido de medicamento cadastrado com sucesso!");
               Console.WriteLine(pedido);
           }

           public void ListarPedidos()
           {
               var pedidos = Carregar();
               if (!pedidos.Any())
               {
                   Console.WriteLine("Não há pedidos de medicamentos.");
                   return;
               }

               Console.WriteLine("\n--- Pedidos de Medicamentos ---");
               foreach (var p in pedidos.OrderByDescending(p => p.DataPedido))
               {
                   Console.WriteLine(p);
               }
           }
       }

       public class Farmacia
       {
           public int Id { get; set; }
           public string Nome { get; set; }
           public string Endereco { get; set; }
           public string Telefone { get; set; }
           public string HorarioFuncionamento { get; set; }
           public string BairroOuRegiao { get; set; }

           public override string ToString()
           {
               return $"[{Id}] {Nome} - {Endereco} ({BairroOuRegiao}) Tel: {Telefone} - Horário: {HorarioFuncionamento}";
           }
       }

       public class GerenciadorFarmacias
       {
           private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "farmacias.json");

           public List<Farmacia> Carregar()
           {
               if (!File.Exists(NomeArquivo))
                   return new List<Farmacia>();

               try
               {
                   string json = File.ReadAllText(NomeArquivo);
                   return JsonSerializer.Deserialize<List<Farmacia>>(json) ?? new List<Farmacia>();
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao carregar farmácias: {ex.Message}");
                   return new List<Farmacia>();
               }
           }

           public void Salvar(List<Farmacia> farmacias)
           {
               try
               {
                   var options = new JsonSerializerOptions { WriteIndented = true };
                   string json = JsonSerializer.Serialize(farmacias, options);
                   File.WriteAllText(NomeArquivo, json);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao salvar farmácias: {ex.Message}");
               }
           }

           public void CadastrarFarmacia()
           {
               var farmacias = Carregar();

               Console.Write("Nome da farmácia: ");
               string nome = Console.ReadLine();

               Console.Write("Endereço: ");
               string endereco = Console.ReadLine();

               Console.Write("Telefone: ");
               string telefone = Console.ReadLine();

               Console.Write("Bairro/Região: ");
               string regiao = Console.ReadLine();

               Console.Write("Horário de funcionamento (ex: 08h às 20h): ");
               string horario = Console.ReadLine();

               int novoId = farmacias.Any() ? farmacias.Max(f => f.Id) + 1 : 1;

               var farmacia = new Farmacia
               {
                   Id = novoId,
                   Nome = nome,
                   Endereco = endereco,
                   Telefone = telefone,
                   BairroOuRegiao = regiao,
                   HorarioFuncionamento = horario
               };

               farmacias.Add(farmacia);
               Salvar(farmacias);

               Console.WriteLine("\n🏥 Farmácia cadastrada com sucesso!");
               Console.WriteLine(farmacia);
           }

           public void ListarFarmacias()
           {
               var farmacias = Carregar();
               if (!farmacias.Any())
               {
                   Console.WriteLine("Não há farmácias cadastradas.");
                   return;
               }

               Console.WriteLine("\n--- Farmácias Cadastradas ---");
               foreach (var f in farmacias.OrderBy(f => f.Nome))
               {
                   Console.WriteLine(f);
               }
           }
       }

       public class HistoricoTratamento
       {
           public int Id { get; set; }
           public string NomePaciente { get; set; }
           public DateTime Data { get; set; }
           public string Tipo { get; set; }
           public string Descricao { get; set; }

           public override string ToString()
           {
               return $"[{Id}] {Data:dd/MM/yyyy} - {NomePaciente} - {Tipo}: {Descricao}";
           }
       }

       public class GerenciadorHistorico
       {
           private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "historico_tratamentos.json");

           public List<HistoricoTratamento> Carregar()
           {
               if (!File.Exists(NomeArquivo))
                   return new List<HistoricoTratamento>();

               try
               {
                   string json = File.ReadAllText(NomeArquivo);
                   return JsonSerializer.Deserialize<List<HistoricoTratamento>>(json) ?? new List<HistoricoTratamento>();
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao carregar histórico: {ex.Message}");
                   return new List<HistoricoTratamento>();
               }
           }

           public void Salvar(List<HistoricoTratamento> historico)
           {
               try
               {
                   var options = new JsonSerializerOptions { WriteIndented = true };
                   string json = JsonSerializer.Serialize(historico, options);
                   File.WriteAllText(NomeArquivo, json);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao salvar histórico: {ex.Message}");
               }
           }

           public void RegistrarEvento()
           {
               var historico = Carregar();

               Console.Write("Nome do paciente: ");
               string paciente = Console.ReadLine();

               Console.Write("Tipo (Consulta, Medicação, Exame, Outro): ");
               string tipo = Console.ReadLine();

               Console.Write("Descrição: ");
               string desc = Console.ReadLine();

               int novoId = historico.Any() ? historico.Max(h => h.Id) + 1 : 1;

               var item = new HistoricoTratamento
               {
                   Id = novoId,
                   NomePaciente = paciente,
                   Tipo = tipo,
                   Descricao = desc,
                   Data = DateTime.Now
               };

               historico.Add(item);
               Salvar(historico);

               Console.WriteLine("\n📚 Evento registrado no histórico!");
               Console.WriteLine(item);
           }

           public void ListarHistorico()
           {
               var historico = Carregar();
               if (!historico.Any())
               {
                   Console.WriteLine("Histórico vazio.");
                   return;
               }

               Console.WriteLine("\n--- Histórico de Tratamentos ---");
               foreach (var h in historico.OrderByDescending(h => h.Data))
               {
                   Console.WriteLine(h);
               }
           }
       }

       public class MonitoramentoFamiliar
       {
           public int Id { get; set; }
           public string NomePaciente { get; set; }
           public string NomeCuidador { get; set; }
           public string TelefoneCuidador { get; set; }
           public string Observacoes { get; set; }
           public bool Ativo { get; set; } = true;

           public override string ToString()
           {
               var status = Ativo ? "Ativo" : "Inativo";
               return $"[{Id}] Paciente: {NomePaciente} - Cuidador: {NomeCuidador} ({TelefoneCuidador}) - {status} - Obs: {Observacoes}";
           }
       }

       public class GerenciadorMonitoramento
       {
           private static readonly string NomeArquivo = Path.Combine(AppContext.BaseDirectory, "monitoramento_familiar.json");

           public List<MonitoramentoFamiliar> Carregar()
           {
               if (!File.Exists(NomeArquivo))
                   return new List<MonitoramentoFamiliar>();

               try
               {
                   string json = File.ReadAllText(NomeArquivo);
                   return JsonSerializer.Deserialize<List<MonitoramentoFamiliar>>(json) ?? new List<MonitoramentoFamiliar>();
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao carregar monitoramento: {ex.Message}");
                   return new List<MonitoramentoFamiliar>();
               }
           }

           public void Salvar(List<MonitoramentoFamiliar> itens)
           {
               try
               {
                   var options = new JsonSerializerOptions { WriteIndented = true };
                   string json = JsonSerializer.Serialize(itens, options);
                   File.WriteAllText(NomeArquivo, json);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Erro ao salvar monitoramento: {ex.Message}");
               }
           }

           public void CadastrarVinculo()
           {
               var itens = Carregar();

               Console.Write("Nome do paciente: ");
              	string paciente = Console.ReadLine();

               Console.Write("Nome do cuidador/familiar: ");
               string cuidador = Console.ReadLine();

               Console.Write("Telefone do cuidador: ");
               string telefone = Console.ReadLine();

               Console.Write("Observações: ");
               string obs = Console.ReadLine();

               int novoId = itens.Any() ? itens.Max(i => i.Id) + 1 : 1;

               var item = new MonitoramentoFamiliar
               {
                   Id = novoId,
                   NomePaciente = paciente,
                   NomeCuidador = cuidador,
                   TelefoneCuidador = telefone,
                   Observacoes = obs,
                   Ativo = true
               };

               itens.Add(item);
               Salvar(itens);

               Console.WriteLine("\n👨‍👩‍👧‍👦 Vinculo de monitoramento cadastrado!");
               Console.WriteLine(item);
           }

           public void ListarVinculos()
           {
               var itens = Carregar();
               if (!itens.Any())
               {
                   Console.WriteLine("Não há vínculos cadastrados.");
                   return;
               }

               Console.WriteLine("\n--- Monitoramento Familiar ---");
               foreach (var i in itens)
               {
                   Console.WriteLine(i);
               }
           }
       }

       public class ProgramMain
       {
           private static GerenciadorDados _gerenciador = new GerenciadorDados();
           private static List<Medicamento> _medicamentos;

           public static void Main(string[] args)
           {
               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("=== CUIDA BEM ===");
                   Console.WriteLine("1 - Controle de Medicamentos (parte já existente)");
                   Console.WriteLine("2 - Gerenciamento de Consultas");
                   Console.WriteLine("3 - Pedidos de Medicamentos");
                   Console.WriteLine("4 - Farmácias Próximas");
                   Console.WriteLine("5 - Histórico de Tratamentos");
                   Console.WriteLine("6 - Monitoramento Familiar");
                   Console.WriteLine("0 - Sair");
                   Console.Write("Escolha uma opção: ");

                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           ExecutarModuloMedicamentos();
                           break;
                       case "2":
                           ExecutarModuloConsultas();
                           break;
                       case "3":
                           ExecutarModuloPedidos();
                           break;
                       case "4":
                           ExecutarModuloFarmacias();
                           break;
                       case "5":
                           ExecutarModuloHistorico();
                           break;
                       case "6":
                           ExecutarModuloMonitoramento();
                           break;
                       case "0":
                           Console.WriteLine("Saindo do sistema...");
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void ExecutarModuloMedicamentos()
           {
               Console.Clear();
               Console.WriteLine("--- Controle de Medicamentos ---");

               _medicamentos = _gerenciador.CarregarMedicamentos();
               Console.WriteLine($" {_medicamentos.Count} medicamento(s) carregado(s).");

               ColetarDadosDoUsuario();

               _gerenciador.SalvarMedicamentos(_medicamentos);

               Console.WriteLine("\n--- Monitoramento de Horários ---");
               IniciarMonitoramento();

               Pausar();
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
                       break;

                   Console.WriteLine("Horário inválido. Use o formato HH:mm (ex: 07:30).");
               }

               int quantidade;
               while (true)
               {
                   Console.Write("Quantidade de comprimidos: ");
                   if (int.TryParse(Console.ReadLine(), out quantidade) && quantidade > 0)
                       break;
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
                       Console.WriteLine($"HORA DO MEDICAMENTO! ({agora:HH\\:mm}) 🔔");
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

           private static void ExecutarModuloConsultas()
           {
               var gerenciador = new GerenciadorConsultas();

               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("--- Gerenciamento de Consultas ---");
                   Console.WriteLine("1 - Cadastrar consulta");
                   Console.WriteLine("2 - Listar consultas");
                   Console.WriteLine("3 - Reagendar consulta");
                   Console.WriteLine("0 - Voltar");
                   Console.Write("Escolha uma opção: ");
                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           gerenciador.CadastrarConsulta();
                           Pausar();
                           break;
                       case "2":
                           gerenciador.ListarConsultas();
                           Pausar();
                       break;
                       case "3":
                           gerenciador.ReagendarConsulta();
                           Pausar();
                           break;
                       case "0":
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void ExecutarModuloPedidos()
           {
               var gerenciador = new GerenciadorPedidosMedicamentos();

               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("--- Pedidos de Medicamentos ---");
                   Console.WriteLine("1 - Cadastrar pedido");
                   Console.WriteLine("2 - Listar pedidos");
                   Console.WriteLine("0 - Voltar");
                   Console.Write("Escolha uma opção: ");
                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           gerenciador.CadastrarPedido();
                           Pausar();
                           break;
                       case "2":
                           gerenciador.ListarPedidos();
                           Pausar();
                           break;
                       case "0":
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void ExecutarModuloFarmacias()
           {
               var gerenciador = new GerenciadorFarmacias();

               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("--- Farmácias Próximas ---");
                   Console.WriteLine("1 - Cadastrar farmácia");
                   Console.WriteLine("2 - Listar farmácias");
                   Console.WriteLine("0 - Voltar");
                   Console.Write("Escolha uma opção: ");
                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           gerenciador.CadastrarFarmacia();
                           Pausar();
                           break;
                       case "2":
                           gerenciador.ListarFarmacias();
                           Pausar();
                           break;
                       case "0":
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void ExecutarModuloHistorico()
           {
               var gerenciador = new GerenciadorHistorico();

               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("--- Histórico de Tratamentos ---");
                   Console.WriteLine("1 - Registrar evento");
                   Console.WriteLine("2 - Listar histórico");
                   Console.WriteLine("0 - Voltar");
                   Console.Write("Escolha uma opção: ");
                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           gerenciador.RegistrarEvento();
                           Pausar();
                           break;
                       case "2":
                           gerenciador.ListarHistorico();
                           Pausar();
                           break;
                       case "0":
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void ExecutarModuloMonitoramento()
           {
               var gerenciador = new GerenciadorMonitoramento();

               while (true)
               {
                   Console.Clear();
                   Console.WriteLine("--- Monitoramento Familiar ---");
                   Console.WriteLine("1 - Cadastrar vínculo cuidador/paciente");
                   Console.WriteLine("2 - Listar vínculos");
                   Console.WriteLine("0 - Voltar");
                   Console.Write("Escolha uma opção: ");
                   var opcao = Console.ReadLine();

                   switch (opcao)
                   {
                       case "1":
                           gerenciador.CadastrarVinculo();
                           Pausar();
                           break;
                       case "2":
                           gerenciador.ListarVinculos();
                           Pausar();
                           break;
                       case "0":
                           return;
                       default:
                           Console.WriteLine("Opção inválida.");
                           Pausar();
                           break;
                   }
               }
           }

           private static void Pausar()
           {
               Console.WriteLine("\nPressione qualquer tecla para continuar...");
               Console.ReadKey();
           }
       }
