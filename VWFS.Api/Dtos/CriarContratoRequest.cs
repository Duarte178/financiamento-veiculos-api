using System;

namespace VWFS.Api.Dtos
{
    public class CriarContratoRequest
    {
        public string ClienteCpfCnpj { get; set; } = "";
        public decimal ValorTotal { get; set; }
        public int PrazoMeses { get; set; }
        public decimal TaxaMensal { get; set; }
        public DateTime DataVencimentoPrimeiraParcela { get; set; }
        public string TipoVeiculo { get; set; } = "";
        public string CondicaoVeiculo { get; set; } = "";
    }
}
