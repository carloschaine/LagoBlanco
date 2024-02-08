namespace LagoBlanco.Web.ViewModels
{
    public class RadialChartVM
    {
        public decimal TotalCount { get; set; }
        public decimal CountInCurrentMonth  { get; set; }
        public bool HasRatioIncrease { get; set; }
        public int[] Series { get; set; }
    }
}
