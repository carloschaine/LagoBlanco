using LagoBlanco.Application.Common.DTO;
using LagoBlanco.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LagoBlanco.Web.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashBoardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetTotalBooking_ChartData()
        {               
            return Json(await _dashboardService.GetTotalBooking_ChartData());
        }
        public async Task<IActionResult> GetRegisteredUser_ChartData()
        {
            return Json(await _dashboardService.GetRegisteredUser_ChartData());
        }
        public async Task<IActionResult> GetRevenue_ChartData()
        {
            return Json( await _dashboardService.GetRevenue_ChartData());
        }


        public async Task<IActionResult> GetBooking_PieChart()
        {
            PieChartDto result =  await _dashboardService.GetBooking_PieChart();
            return Json(result);
        }


        public async Task<IActionResult> GetMemberAndBooking_LineChart()
        {
            return Json(await _dashboardService.GetMemberAndBooking_LineChart());
        }



    }
}
