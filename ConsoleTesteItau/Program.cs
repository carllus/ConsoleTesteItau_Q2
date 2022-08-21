using ConsoleTesteItau.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace ConsoleTesteItau
{
    class Program
    {
        static void Main(string[] args)
        {
            int segundos = 120;
            Console.WriteLine("Iniciando CSV's!");
            var dadosMoedas = GetDadosMoedaCSV();
            Console.WriteLine("...processando...");
            var dadosCotacao = GetDadosCotacaoCSV();
            Console.WriteLine("Finalizados CSV's!");
            Console.WriteLine("Iniciando aplicação!");
            while (true)
            {
                List<MoedaCotacao> moedaCotacao = new List<MoedaCotacao>();
                var cliente = new RestClient("https://localhost:44387/Itens/");
                var request = new RestRequest("GetItemFila");
                var response = cliente.Execute(request);
                if (!response.Content.Contains("NAO EXISTE MOEDA"))
                {
                    JSonItem jsonItem = JsonSerializer.Deserialize<JSonItem>(response.Content);

                    Console.WriteLine("moeda: " + jsonItem.moeda);
                    Console.WriteLine("DATA_INI: " + jsonItem.data_inicio + " DATA_FIM: " + jsonItem.data_fim);

                    var elementos = dadosMoedas.Where(x => jsonItem.data_inicio <= x.DataReferencia && x.DataReferencia <= jsonItem.data_fim).ToList();
                    foreach(var elemento in elementos)
                    {
                        Console.WriteLine(elemento.Id + " - " + elemento.DataReferencia);
                        int codCotacao = DePara(elemento.Id);
                        var dadoCotacao = dadosCotacao.Where(x => x.Codigo == codCotacao && x.Data == elemento.DataReferencia).First();
                        Console.WriteLine("Cotação: "+ dadoCotacao.Valor);

                        moedaCotacao.Add(new MoedaCotacao() { IdMoeda = elemento.Id, Data = elemento.DataReferencia, Cotacao = dadoCotacao.Valor });
                    }

                    EscreverCSV(moedaCotacao);

                    Console.WriteLine("Aguardando {0} segundos", segundos);
                    Thread.Sleep(segundos * 1000);
                }
                else
                {
                    Console.WriteLine("API NAO RETORNOU ELEMENTO");
                    Thread.Sleep(segundos * 1000);
                }
            }
            
        }

        static List<Moeda> GetDadosMoedaCSV ()
        {
            List<Moeda> dadosMoeda = new List<Moeda>();
            using (var reader = new StreamReader("files/DadosMoeda.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (values[0].ToString() != "ID_MOEDA")
                    {
                        dadosMoeda.Add(new Moeda() { Id = values[0], DataReferencia = ConvertDate(values[1]) });
                    }
                }
            }
            return dadosMoeda;
        }

        static void EscreverCSV(List<MoedaCotacao> moedaCotacao)
        {
            File.WriteAllLines("Resultado_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", moedaCotacao.Select(resultado => (string)resultado.IdMoeda + ";" + resultado.Data + ";" + resultado.Cotacao).ToList());
        }

        static List<Cotacao> GetDadosCotacaoCSV()
        {
            List<Cotacao> dadosCotacao = new List<Cotacao>();
            using (var reader = new StreamReader("files/DadosCotacao.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (values[0].ToString() != "vlr_cotacao")
                    {
                        dadosCotacao.Add(new Cotacao() { Valor = Convert.ToDecimal(values[0]), Codigo = Convert.ToInt32(values[1]), Data = ConvertDate(values[2]) });
                    }
                }
            }
            return dadosCotacao;
        }

        static int DePara(string idMoeda)
        {
            using (var reader = new StreamReader("files/DePara.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (values[0].ToString() == idMoeda)
                    {
                        return Convert.ToInt32(values[1]);
                    }
                }
            }
            return -1;
        }

        static DateTime ConvertDate(string data)
        {
            CultureInfo culture = new CultureInfo("es-ES");
            return DateTime.Parse(data, culture);
        }
    }
}
