using LagoBlanco.Application.Common.DTO;
using LagoBlanco.Application.Common.DTO.Chart;

namespace LagoBlanco.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialChartDto> GetTotalBooking_ChartData();
        Task<RadialChartDto> GetRegisteredUser_ChartData();
        Task<RadialChartDto> GetRevenue_ChartData();
        Task<PieChartDto>    GetBooking_PieChart();
        Task<LineChartDto>   GetMemberAndBooking_LineChart();
    }
}
