namespace VWFS.Api.Dtos
{
    public class ResumoClienteResponse
    {
        public string CpfCnpj { get; set; } = "";
        public int QuantidadeContratos { get; set; }
        public int QuantidadeContratosAtivos { get; set; }
        public int TotalParcelas { get; set; }
        public int ParcelasPagas { get; set; }
        public int ParcelasEmAtraso { get; set; }
        public int ParcelasAVencer { get; set; }
        public double PercentualPagasEmDia { get; set; }
    }
}
