using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Projeto_Classes.Classes;
using Projeto_Classes.Classes.Gerencial;
using System.Globalization;
using System.Data;
using System.Collections;
using System.Xml;

namespace Escuta_Queclink
{
    class Program
    {
        private static ArrayList contas = new ArrayList();

        private static void Main()
        {

            #region Contas HERE

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("http://rastrear.a3rastreadores.com.br/contas_here/contas_here.xml");

            XmlNodeList coluna = xDoc.GetElementsByTagName("coluna");
            XmlNodeList app_id = xDoc.GetElementsByTagName("app_id");
            XmlNodeList app_code = xDoc.GetElementsByTagName("app_code");
            XmlNodeList inicio = xDoc.GetElementsByTagName("inicio");
            XmlNodeList fim = xDoc.GetElementsByTagName("fim");

            for (int i = 0; i < coluna.Count; i++)
            {
                ArrayList itens = new ArrayList();
                itens.Add(coluna[i].InnerText);
                itens.Add(app_id[i].InnerText);
                itens.Add(app_code[i].InnerText);
                itens.Add(inicio[i].InnerText);
                itens.Add(fim[i].InnerText);
                contas.Add(itens);
            }

            #endregion

            TcpListener socket = new TcpListener(IPAddress.Any, 7100);
            try
            {
                Console.WriteLine("Conectado !");

                //string mensagem_traduzida = "+RESP:GTFTP,3C0303,3594644038007972,,,A3_20171009192608.jpg,0,0.0,216,475.4,-51.417806,-22.135279,20171009192705,0724,0003,0206,4E2D,00,20171009192746,0E45$";
                /*string mensagem_traduzida = "+RESP:GTFTP,3C0303,3594644038007972,,0,,20171009192608,0,0.0,216,475.4,-51.417806,-22.135279,20171009192705,0724,0003,0206,4E2D,00,,,,,20171009192708,0E45$";
                Console.WriteLine("\n" + mensagem_traduzida);
                var array_mensagem = mensagem_traduzida.Split(',');

                Dados lista = new Dados();

                string[] tipo_mensagem = array_mensagem[0].Split(':');
                if (tipo_mensagem[0] == "+RESP")
                {
                    lista.Tipo_Mensagem = tipo_mensagem[1];

                    if (lista.Tipo_Mensagem == "GTFTP")
                    {
                        #region Preenchendo Objeto
                        if (mensagem_traduzida.Contains(".jpg"))
                        {
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Nome_Arquivo = array_mensagem[5];

                            lista.GPS_Precisao = array_mensagem[6];
                            lista.Velocidade = array_mensagem[7].Split('.')[0];
                            lista.Direcao = array_mensagem[8];

                            lista.Longitude = array_mensagem[10];
                            lista.Latitude = array_mensagem[11];
                            string data_hora = array_mensagem[12];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            GravarFoto(lista);
                        }
                        else
                        {
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Nome_Arquivo = "A3_" + array_mensagem[6] + ".jpg";

                            lista.GPS_Precisao = array_mensagem[7];
                            lista.Velocidade = array_mensagem[8].Split('.')[0];
                            lista.Direcao = array_mensagem[9];

                            lista.Longitude = array_mensagem[11];
                            lista.Latitude = array_mensagem[12];
                            string data_hora = array_mensagem[13];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            GravarFoto(lista);
                        }
                        #endregion
                    }
                }*/

                socket.Start();

                while (true)
                {
                    TcpClient client = socket.AcceptTcpClient();

                    Thread tcpListenThread = new Thread(TcpListenThread);
                    tcpListenThread.Start(client);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                Thread tcpListenThread = new Thread(Main);
                tcpListenThread.Start();
                socket.Stop();
            }
        }

        private static void TcpListenThread(object param)
        {
            TcpClient client = (TcpClient)param;
            NetworkStream stream;
            stream = client.GetStream();

            //Thread tcpLSendThread = new Thread(new ParameterizedThreadStart(TcpLSendThread));

            byte[] bytes = new byte[99999];
            int i;
            bool from_raster = true;
            stream.ReadTimeout = 1200000;
            try
            {
                while (from_raster && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string mensagem_traduzida = Encoding.UTF8.GetString(bytes, 0, i);
                    //Console.WriteLine("\n" + mensagem_traduzida);                    
                    var array_mensagem = mensagem_traduzida.Split(',');

                    Dados lista = new Dados();

                    string[] tipo_mensagem = array_mensagem[0].Split(':');
                    if (tipo_mensagem[0] == "+RESP")
                    {
                        lista.Tipo_Mensagem = tipo_mensagem[1];                        
                        //EVENTOS

                        //GTSTC - OK
                        //GTMPF - GTMPN - GTBTC - OK
                        //GTIGN - GTIGF -OK
                        //GTSTT - OK
                        //GTPFA - CURTA
                        //GTPNA - CURTA
                        //GTPHL - ANTES DE ENVIAR FOTO - NÃO TRATADO
                        //GTFTP - OK - CONFIRMAÇÃO DE ENVIO DE FOTO

                        //POSIÇÃO
                        //GTFRI - OK

                        //PRÓXIMO GRUPO O PADRÃO DE MENSAGEM É O MESMO
                        //GTIGL - OK
                        //GTTOW - OK
                        //GTDIS - OK
                        //GTIOB - OK
                        //GTSPD - OK
                        //GTSOS - OK
                        //GTRTL - OK
                        //GTDOG - OK
                        //GTHBM - OK

                        //PRÓXIMO GRUPO O PADRÃO DE MENSAGEM É O MESMO
                        //GTEPS - OK
                        //GTGEO - OK

                        //GTLBC - OK

                        //GTIDA - OK

                        //GTCAN

                        //CERCAS DO RASTREADOR
                        //GTGIN
                        //GTGOT

                        #region SALVANDO FOTO
                        if (lista.Tipo_Mensagem == "GTFTP")
                        {
                            #region Preenchendo Objeto
                            if (mensagem_traduzida.Contains(".jpg"))
                            {
                                lista.Protocolo = array_mensagem[1];
                                lista.Imei = array_mensagem[2];
                                lista.Nome_Rastreador = array_mensagem[3];
                                lista.Nome_Arquivo = array_mensagem[5];

                                lista.GPS_Precisao = array_mensagem[6];
                                lista.Velocidade = array_mensagem[7].Split('.')[0];
                                lista.Direcao = array_mensagem[8];

                                /*lista.Longitude = array_mensagem[9];
                                lista.Latitude = array_mensagem[10];
                                string data_hora = array_mensagem[11];*/
                                lista.Longitude = array_mensagem[10];
                                lista.Latitude = array_mensagem[11];
                                string data_hora = array_mensagem[12];

                                data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                                lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                                lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                                GravarFoto(lista);
                            }
                            else
                            {
                                lista.Protocolo = array_mensagem[1];
                                lista.Imei = array_mensagem[2];
                                lista.Nome_Rastreador = array_mensagem[3];
                                lista.Nome_Arquivo = "A3_" + array_mensagem[6] + ".jpg";

                                lista.GPS_Precisao = array_mensagem[7];
                                lista.Velocidade = array_mensagem[8].Split('.')[0];
                                lista.Direcao = array_mensagem[9];

                                lista.Longitude = array_mensagem[11];
                                lista.Latitude = array_mensagem[12];
                                string data_hora = array_mensagem[13];
                                data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                                lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                                lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                                GravarFoto(lista);
                            }
                            #endregion
                        }
                        #endregion

                        #region POSIÇÃO
                        if (lista.Tipo_Mensagem == "GTIGL"
                            || lista.Tipo_Mensagem == "GTTOW"
                            || lista.Tipo_Mensagem == "GTDIS"
                            || lista.Tipo_Mensagem == "GTIOB"
                            || lista.Tipo_Mensagem == "GTSPD"
                            || lista.Tipo_Mensagem == "GTSOS"
                            || lista.Tipo_Mensagem == "GTRTL"
                            || lista.Tipo_Mensagem == "GTDOG"
                            || lista.Tipo_Mensagem == "GTHBM")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];

                            lista.Input2 = array_mensagem[4];
                            lista.Id = array_mensagem[5];
                            lista.Numero = array_mensagem[6];

                            lista.GPS_Precisao = array_mensagem[7];
                            lista.Velocidade = array_mensagem[8].Split('.')[0];
                            lista.Direcao = array_mensagem[9];
                            lista.Altitude = array_mensagem[10];
                            lista.Longitude = array_mensagem[11];
                            lista.Latitude = array_mensagem[12];
                            string data_hora = array_mensagem[13];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            lista.Mcc = array_mensagem[14]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[15]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[16]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[17]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[18];
                            lista.Hodometro = array_mensagem[19]; //Mileage

                            lista.Data_Envio = array_mensagem[20];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[21], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[21];


                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Direcao;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        if (lista.Tipo_Mensagem == "GTFRI")
                        {
                            #region Preenchendo Objeto

                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Tensao = (array_mensagem[4] == "" ? "" : (Convert.ToInt32(array_mensagem[4]) / 1000.0).ToString());
                            lista.Id = array_mensagem[5]; //Report Id - Vide Pag. 125
                            lista.Numero = array_mensagem[6]; //Sempre valor '1' -> ignorado com sucesso!
                            lista.GPS_Precisao = array_mensagem[7];
                            lista.Velocidade = array_mensagem[8].Split('.')[0];
                            lista.Direcao = array_mensagem[9];
                            lista.Altitude = array_mensagem[10];
                            lista.Longitude = array_mensagem[11];
                            lista.Latitude = array_mensagem[12];
                            string data_hora = array_mensagem[13];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Mcc = array_mensagem[14]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[15]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[16]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[17]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[18];
                            lista.Hodometro = array_mensagem[19]; //Mileage
                            lista.Horimetro = array_mensagem[20]; //Hour Meter Count
                            lista.Input2 = array_mensagem[21];
                            lista.Input3 = array_mensagem[22];
                            lista.Porcentagem_Bateria = array_mensagem[23];
                            lista.Status_Dispositivo = array_mensagem[24];
                            // 210100
                            // 11 - desligado/parado
                            // 12 - desligado/movimento
                            // 21 - ligado/parado
                            // 22 - ligado/movimento

                            lista.Input4 = array_mensagem[25];
                            lista.Input5 = array_mensagem[26];
                            lista.Input6 = array_mensagem[27];

                            lista.Data_Envio = array_mensagem[28];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[29], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[29];


                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Hodometro + ";" +
                                                lista.Horimetro + ";" +
                                                lista.Porcentagem_Bateria + ";" +
                                                lista.Status_Dispositivo + ";" +
                                                lista.Direcao + ";" +
                                                lista.Numero_Mensagem;

                            Console.WriteLine("\n" + mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }


                        if (lista.Tipo_Mensagem == "GTEPS" || lista.Tipo_Mensagem == "GTGEO")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Tensao = (array_mensagem[4] == "" ? "" : (Convert.ToInt32(array_mensagem[4]) / 1000.0).ToString());
                            lista.Id = array_mensagem[5]; //Report Id - Vide Pag. 125
                            lista.Numero = array_mensagem[6]; //Sempre valor '1' -> ignorado com sucesso!
                            lista.GPS_Precisao = array_mensagem[7];
                            lista.Velocidade = array_mensagem[8].Split('.')[0];
                            lista.Direcao = array_mensagem[9];
                            lista.Altitude = array_mensagem[10];
                            lista.Longitude = array_mensagem[11];
                            lista.Latitude = array_mensagem[12];
                            string data_hora = array_mensagem[13];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Mcc = array_mensagem[14]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[15]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[16]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[17]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[18];
                            lista.Hodometro = array_mensagem[19]; //Mileage
                            lista.Data_Envio = array_mensagem[20];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[21], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[21];


                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Hodometro + ";" +
                                                lista.Direcao + ";" +
                                                lista.Numero_Mensagem;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }


                        if (lista.Tipo_Mensagem == "GTLBC")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Numero_Chip = array_mensagem[4];
                            lista.GPS_Precisao = array_mensagem[5];
                            lista.Velocidade = array_mensagem[6].Split('.')[0];
                            lista.Direcao = array_mensagem[7];
                            lista.Altitude = array_mensagem[8];
                            lista.Longitude = array_mensagem[9];
                            lista.Latitude = array_mensagem[10];
                            string data_hora = array_mensagem[11];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Mcc = array_mensagem[12]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[13]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[14]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[15]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[16];
                            lista.Data_Envio = array_mensagem[17];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[18], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[18];


                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Hodometro + ";" +
                                                lista.Direcao + ";" +
                                                lista.Numero_Mensagem;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        if (lista.Tipo_Mensagem == "GTIDA")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];

                            lista.Input2 = array_mensagem[4];
                            lista.Id = array_mensagem[5];
                            lista.Numero = array_mensagem[6];

                            lista.GPS_Precisao = array_mensagem[8];
                            lista.Velocidade = array_mensagem[9].Split('.')[0];
                            lista.Direcao = array_mensagem[10];
                            lista.Altitude = array_mensagem[11];
                            lista.Longitude = array_mensagem[12];
                            lista.Latitude = array_mensagem[13];
                            string data_hora = array_mensagem[14];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            lista.Mcc = array_mensagem[15]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[16]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[17]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[18]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[19];
                            lista.Hodometro = array_mensagem[20]; //Mileage

                            lista.Input2 = array_mensagem[21];
                            lista.Input3 = array_mensagem[22];
                            lista.Input4 = array_mensagem[23];
                            lista.Input5 = array_mensagem[24];

                            lista.Data_Envio = array_mensagem[25];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[26], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[26];

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Direcao;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        if (lista.Tipo_Mensagem == "GTGES")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Input2 = array_mensagem[4];
                            lista.Id = array_mensagem[5];
                            lista.Trigger = array_mensagem[6];
                            lista.Radius = array_mensagem[7];
                            lista.Check_Interval = array_mensagem[8];
                            lista.Numero = array_mensagem[9];

                            lista.GPS_Precisao = array_mensagem[10];
                            lista.Velocidade = array_mensagem[11].Split('.')[0];
                            lista.Direcao = array_mensagem[12];
                            lista.Altitude = array_mensagem[13];
                            lista.Longitude = array_mensagem[14];
                            lista.Latitude = array_mensagem[15];
                            string data_hora = array_mensagem[16];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            lista.Mcc = array_mensagem[17]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[18]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[19]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[20]; //CELL ID (HEX) -> ignorado com sucesso!
                            lista.Input1 = array_mensagem[21];
                            lista.Hodometro = array_mensagem[22]; //Mileage
                            lista.Data_Envio = array_mensagem[23];
                            //lista.Numero_Mensagem = int.Parse(array_mensagem[24], System.Globalization.NumberStyles.HexNumber);
                            lista.Numero_Mensagem = array_mensagem[24];


                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Direcao;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }
                        

                        #endregion
                        
                        #region EVENTOS
                        /*

                        // EVENTOS
                        if (lista.Tipo_Mensagem == "GTMPF" || lista.Tipo_Mensagem == "GTMPN" || lista.Tipo_Mensagem == "GTBTC")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.GPS_Precisao = array_mensagem[4];
                            lista.Velocidade = array_mensagem[5].Split('.')[0];
                            lista.Direcao = array_mensagem[6];

                            lista.Longitude = array_mensagem[8];
                            lista.Latitude = array_mensagem[9];
                            string data_hora = array_mensagem[10];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Direcao;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        //JAMMER
                        if (lista.Tipo_Mensagem == "GTSTC")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Jammer = Convert.ToInt32(array_mensagem[4]);
                            lista.GPS_Precisao = array_mensagem[5];
                            lista.Velocidade = array_mensagem[6].Split('.')[0];
                            lista.Direcao = array_mensagem[7];

                            lista.Longitude = array_mensagem[9];
                            lista.Latitude = array_mensagem[10];
                            string data_hora = array_mensagem[11];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Direcao;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        // EVENTO DE STATUS DE IGNIÇÃO
                        if (lista.Tipo_Mensagem == "GTSTT")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];
                            lista.Status_Dispositivo = array_mensagem[4];
                            // 16 - desligado/movimento - está sendo rebocado
                            // 11 - desligado/parado
                            // 12 - desligado/movimento - confirmação de rebocação
                            // 21 - ligado/parado
                            // 22 - ligado/movimento
                            // 41 - sensor parado
                            // 42 - sensor de movimento
                            lista.Ignicao = (lista.Status_Dispositivo == "21" || lista.Status_Dispositivo == "22") ? true : false;

                            lista.GPS_Precisao = array_mensagem[5];
                            lista.Velocidade = array_mensagem[6].Split('.')[0];
                            lista.Direcao = array_mensagem[7];
                            lista.Altitude = array_mensagem[8];
                            lista.Longitude = array_mensagem[9];
                            lista.Latitude = array_mensagem[10];
                            string data_hora = array_mensagem[11];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Mcc = array_mensagem[12]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[13]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[14]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[15]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[16];
                            //lista.Hodometro = array_mensagem[19]; //Mileage
                            //lista.Horimetro = array_mensagem[20]; //Hour Meter Count
                            //lista.Input2 = array_mensagem[21];
                            //lista.Input3 = array_mensagem[22];
                            //lista.Porcentagem_Bateria = array_mensagem[23];
                            //lista.Status_Dispositivo = array_mensagem[24];
                            // 210100
                            // 11 - desligado/parado
                            // 12 - desligado/movimento
                            // 21 - ligado/parado
                            // 22 - ligado/movimento

                            //lista.Tensao = (array_mensagem[4] == "" ? "" : (Convert.ToInt32(array_mensagem[4]) / 1000.0).ToString());
                            //lista.Id = array_mensagem[5]; //Report Id - Vide Pag. 125
                            //lista.Numero = array_mensagem[6]; //Sempre valor '1' -> ignorado com sucesso!


                            //lista.Input4 = array_mensagem[25];
                            //lista.Input5 = array_mensagem[26];
                            //lista.Input6 = array_mensagem[27];

                            lista.Data_Envio = array_mensagem[17];
                            lista.Numero_Mensagem = int.Parse(array_mensagem[18], System.Globalization.NumberStyles.HexNumber);


                            //lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Hodometro + ";" +
                                                lista.Horimetro + ";" +
                                //lista.Porcentagem_Bateria + ";" +
                                //lista.Status_Dispositivo + ";" +
                                                lista.Direcao + ";" +
                                                lista.Numero_Mensagem;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }

                        // TEMPO QUE A IGNIÇÃO FICOU DESLIGADA/LIGADA
                        if (lista.Tipo_Mensagem == "GTIGN" || lista.Tipo_Mensagem == "GTIGF")
                        {
                            #region Preenchendo Objeto
                            lista.Protocolo = array_mensagem[1];
                            lista.Imei = array_mensagem[2];
                            lista.Nome_Rastreador = array_mensagem[3];

                            lista.Tempo_Ignicao = array_mensagem[4];
                            lista.Status_Tempo_Ignicao = lista.Tipo_Mensagem == "GTIGN" ? false : true;

                            lista.GPS_Precisao = array_mensagem[5];
                            lista.Velocidade = array_mensagem[6].Split('.')[0];
                            lista.Direcao = array_mensagem[7];
                            lista.Altitude = array_mensagem[8];
                            lista.Longitude = array_mensagem[9];
                            lista.Latitude = array_mensagem[10];
                            string data_hora = array_mensagem[11];
                            data_hora = data_hora.Substring(0, 4) + "-" + data_hora.Substring(4, 2) + "-" + data_hora.Substring(6, 2) + " " + data_hora.Substring(8, 2) + ":" + data_hora.Substring(10, 2) + ":" + data_hora.Substring(12, 2);
                            lista.Data_Rastreador = Convert.ToDateTime(data_hora);

                            lista.Mcc = array_mensagem[12]; //MOBILE CONTRY CODE -> ignorado com sucesso!
                            lista.Mnc = array_mensagem[13]; //MOBILE NETWORK CODE -> ignorado com sucesso!
                            lista.Lac = array_mensagem[14]; //LOCATION AREA CODE (HEX) -> ignorado com sucesso!
                            lista.Id_Chip = array_mensagem[15]; //CELL ID (HEX) -> ignorado com sucesso!

                            lista.Input1 = array_mensagem[16];
                            lista.Horimetro = array_mensagem[17]; //Hour Meter Count
                            lista.Hodometro = array_mensagem[18]; //Mileage
                            //lista.Input2 = array_mensagem[21];
                            //lista.Input3 = array_mensagem[22];
                            //lista.Porcentagem_Bateria = array_mensagem[23];
                            //lista.Status_Dispositivo = array_mensagem[24];
                            // 210100
                            // 11 - desligado/parado
                            // 12 - desligado/movimento
                            // 21 - ligado/parado
                            // 22 - ligado/movimento

                            //lista.Tensao = (array_mensagem[4] == "" ? "" : (Convert.ToInt32(array_mensagem[4]) / 1000.0).ToString());
                            //lista.Id = array_mensagem[5]; //Report Id - Vide Pag. 125
                            //lista.Numero = array_mensagem[6]; //Sempre valor '1' -> ignorado com sucesso!


                            //lista.Input4 = array_mensagem[25];
                            //lista.Input5 = array_mensagem[26];
                            //lista.Input6 = array_mensagem[27];

                            lista.Data_Envio = array_mensagem[19];
                            lista.Numero_Mensagem = int.Parse(array_mensagem[20], System.Globalization.NumberStyles.HexNumber);


                            lista.Ignicao = Convert.ToInt32(lista.Velocidade) > 0;

                            string mensagem = "QCK75GV;" +
                                                lista.Tipo_Mensagem + ";" +
                                                lista.Imei + ";" +
                                                lista.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                                lista.Latitude + ";" +
                                                lista.Longitude + ";" +
                                                lista.Velocidade + ";" +
                                                (lista.Ignicao ? "1" : "0") + ";" +
                                                lista.Hodometro + ";" +
                                                lista.Horimetro + ";" +
                                //lista.Porcentagem_Bateria + ";" +
                                //lista.Status_Dispositivo + ";" +
                                                lista.Direcao + ";" +
                                                lista.Numero_Mensagem;

                            Console.WriteLine(mensagem);

                            Gravar(lista, mensagem);
                            #endregion
                        }
                        */
                        #endregion
                        
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message.ToString());
                client.Close();
            }
            client.Close();
        }

        private static void Gravar(Dados objeto, string mensagem)
        {
            try
            {
                var m = new Mensagens();
                var r = new Rastreador();
                r.PorId(objeto.Imei);

                m.Data_Rastreador = objeto.Data_Rastreador.ToString("yyyyMMdd HH:mm:ss");
                m.Data_Gps = objeto.Data_Rastreador.ToString("yyyy-MM-dd HH:mm:ss");
                m.Data_Recebida = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                m.ID_Rastreador = objeto.Imei;
                m.Mensagem = mensagem;
                m.Ras_codigo = r.Codigo;
                m.Tipo_Mensagem = "STT";
                m.Latitude = objeto.Latitude;
                m.Longitude = objeto.Longitude;
                m.Tipo_Alerta = "";
                m.Velocidade = objeto.Velocidade;
                m.Vei_codigo = r.Vei_codigo != 0 ? r.Vei_codigo : 0;
                m.Ignicao = objeto.Ignicao;
                m.Hodometro = "";
                m.Bloqueio = false;
                m.Sirene = false;
                m.Tensao = "0";
                m.Horimetro = 0;
                m.CodAlerta = 0;
                m.Endereco = Util.BuscarEndereco(m.Latitude, m.Longitude, contas);

                #region Gravar
                if (m.Gravar())
                {
                    m.Tipo_Mensagem = "EMG";

                    if (r.veiculo != null)
                    {
                        //Verifica Area de Risco/Cerca
                        Mensagens.EventoAreaCerca(m);

                        //Evento Por E-mail
                        var corpoEmail = m.Tipo_Alerta + "<br /> Endereço: " + m.Endereco;
                        Mensagens.EventoPorEmail(m.Vei_codigo, m.CodAlerta, corpoEmail);
                    }

                    #region Velocidade
                    if (r.Vei_codigo != 0)
                    {
                        var veiculo = Veiculo.BuscarVeiculoVelocidade(m.Vei_codigo);
                        var velocidade_nova = Convert.ToDecimal(veiculo.vei_velocidade);
                        if (velocidade_nova < Convert.ToDecimal(m.Velocidade) && velocidade_nova > 0)
                        {
                            m.Tipo_Mensagem = "EVT";
                            m.Tipo_Alerta = "Veículo Ultrapassou a Velocidade";
                            m.CodAlerta = 23;
                            m.GravarEvento();

                            //Evento Por E-mail
                            var corpoEmail = m.Tipo_Alerta + "<br /> Velocidade: " + m.Velocidade + "<br /> Endereço: " + m.Endereco;
                            Mensagens.EventoPorEmail(m.Vei_codigo, m.CodAlerta, corpoEmail);
                        }
                    }
                    #endregion

                }
                #endregion
            }
            catch (Exception e)
            {
                //LogException.GravarException("Erro: " + ex.Message.ToString() + " - Mensagem: " + (ex.InnerException != null ? ex.InnerException.ToString() : " Valor nulo na mensagem "), 12, "Escuta Suntech Novo - Método " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                StreamWriter txt = new StreamWriter("erros_01.txt", true);
                txt.WriteLine("ERRO: " + e.Message.ToString());
                txt.Close();
            }
        }


        private static void GravarFoto(Dados objeto)
        {
            try
            {
                var r = new Rastreador_Imagem();

                r.data_img = objeto.Data_Rastreador.AddHours(-3);
                r.ras_id = objeto.Imei;
                r.nome_img = objeto.Nome_Arquivo;
                r.latitude_img = objeto.Latitude;
                r.longitude_img = objeto.Longitude;

                #region Gravar
                if (r.Gravar())
                {
                    string mensagem = "QCK75GV;GTFTP;IMG:" + r.nome_img + ";" +
                                        objeto.Imei + ";" +
                                        objeto.Data_Rastreador.ToString("yyyyMMdd;HH:mm:ss") + ";" +
                                        objeto.Latitude + ";" +
                                        objeto.Longitude + ";";

                    Console.WriteLine("\n" + mensagem);
                }
                #endregion
            }
            catch (Exception e)
            {
                //LogException.GravarException("Erro: " + ex.Message.ToString() + " - Mensagem: " + (ex.InnerException != null ? ex.InnerException.ToString() : " Valor nulo na mensagem "), 12, "Escuta Suntech Novo - Método " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                StreamWriter txt = new StreamWriter("erros_01.txt", true);
                txt.WriteLine("ERRO: " + e.Message.ToString());
                txt.Close();
            }
        }

    }
}
