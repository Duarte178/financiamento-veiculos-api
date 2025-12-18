using System;

namespace VWFS.Api.Dtos
{
    public class RegistrarPagamentoRequest
    {
        public int NumeroParcela { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
