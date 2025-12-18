using System;

namespace VWFS.Api.Models
{
    public class Pagamento
    {
        public Guid Id { get; set; }
        public int NumeroParcela { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
