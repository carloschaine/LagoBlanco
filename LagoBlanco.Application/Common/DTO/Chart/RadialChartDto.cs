namespace LagoBlanco.Application.Common.DTO.Chart
{
    public class RadialChartDto
    {
        public decimal TotalCount { get; set; }
        public decimal CountInCurrentMonth { get; set; }
        public bool HasRatioIncrease { get; set; }
        public int[] Series { get; set; }
    }
}
