using System;
using System.Collections.Generic;

namespace VWFS.Api.Models
{
    public class Contrato
    {
        public Guid Id { get; set; }
        public string ClienteCpfCnpj { get; set; } = "";
        public decimal ValorTotal { get; set; } // Extra (não atrapalha)
        public int PrazoMeses { get; set; }
        public decimal TaxaMensal { get; set; }
        public DateTime DataVencimentoPrimeiraParcela { get; set; }
        public string TipoVeiculo { get; set; } = "";
        public string CondicaoVeiculo { get; set; } = "";
        public List<Pagamento> Pagamentos { get; set; } = new();
    }
}
