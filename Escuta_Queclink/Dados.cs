using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escuta_Queclink
{
    class Dados
    {
        public string Tipo_Mensagem { get; set; }
        public string Protocolo { get; set; }
        public string Imei { get; set; }
        public string Nome_Rastreador { get; set; }
        public string Tensao { get; set; }
        public string Id { get; set; }
        public string Trigger { get; set; }
        public string Radius { get; set; }
        public string Check_Interval { get; set; }
        public string Numero { get; set; }
        public string Numero_Chip { get; set; }
        public string GPS_Precisao { get; set; }
        public string Velocidade { get; set; }
        public string Direcao { get; set; } //Azimuth
        public string Altitude { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime Data_Rastreador { get; set; }
        public string Mcc { get; set; }
        public string Mnc { get; set; }
        public string Lac { get; set; }
        public string Id_Chip { get; set; }
        public string Input1 { get; set; }
        public string Hodometro { get; set; }
        public string Horimetro { get; set; }
        public string Input2 { get; set; }
        public string Input3 { get; set; }
        public string Porcentagem_Bateria { get; set; }
        public string Status_Dispositivo { get; set; }
        public string Input4 { get; set; }
        public string Input5 { get; set; }
        public string Input6 { get; set; }
        public string Data_Envio { get; set; }
        public int Numero_Mensagem { get; set; }
        public string Nome_Arquivo { get; set; }

        public bool Ignicao { get; set; }
        public string Tempo_Ignicao { get; set; }
        public bool Status_Tempo_Ignicao { get; set; }
        public int Jammer { get; set; } // 1 - SAIU,  2 - ENTROU
    }
}
