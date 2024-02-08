using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LagoBlanco.Web.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month==1 ? 12 : DateTime.Now.Month -1;
        static int previousYear = DateTime.Now.Year - DateTime.Now.Month==1 ? 0 : 1;
        private readonly DateTime previousMonthStartDate = new(previousYear, previousMonth, 1);
        private readonly DateTime currentMonthStartDate  = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashBoardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetTotalBooking_ChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !(b.Status == SD.StatusPending || 
                                                                b.Status == SD.StatusCancelled) );
            var countByCurrentMonth  = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate &&
                                                                u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate &&
                                                                u.BookingDate <= currentMonthStartDate);
            return Json(
                GetRadialCartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth));
        }


        public async Task<IActionResult> GetRegisteredUser_ChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth  = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate &&
                                                             u.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate &&
                                                             u.CreatedAt <= currentMonthStartDate);
            return Json(
               GetRadialCartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }


        public async Task<IActionResult> GetRevenue_ChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !(b.Status == SD.StatusPending ||
                                                                b.Status == SD.StatusCancelled));
            var totalRevenue = Convert.ToInt32( totalBookings.Sum(u => u.TotalCost));
            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate &&
                                                               u.BookingDate <= DateTime.Now)
                                                   .Sum(u=>u.TotalCost);
            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate &&
                                                                u.BookingDate <= currentMonthStartDate)
                                                   .Sum(u => u.TotalCost);
            return Json(
               GetRadialCartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBooking_PieChart()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b =>
                                    b.BookingDate >= DateTime.Now.AddDays(-30) &&
                                    !(b.Status == SD.StatusPending || b.Status == SD.StatusCancelled));
            var customerWithOneBooking = totalBookings.GroupBy(u => u.UserId)
                                                      .Where(x => x.Count() == 1)
                                                      .Select(x => x.Key)
                                                      .ToList();
            //---
            int bookingsByNewCustomer = 3; // customerWithOneBooking.Count();
            int bookingsByReturnedCustomer = 6; // totalBookings.Count() - bookingsByNewCustomer;
            //---
            PieChartVM pieChartVM = new() {
                Series = [bookingsByNewCustomer, bookingsByReturnedCustomer],
                Labels = ["New Customer", "Returned Customer"]
            };
            //--- 
            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndBooking_LineChart()
        {

            ChartData chartData1 = new() { Name = "Usuarios", Data = [4, 6, 5, 3]};
            ChartData chartData2 = new() { Name = "Reservas", Data = [7, 2, 4, 6] };
            //---
            LineChartVM lineChartVM = new LineChartVM() {
                Categories = ["1er semana", "2da semana", "3er semana", "4ta semana"],
                Series = [chartData1, chartData2]
            }; 
            //--- 
            return Json(lineChartVM);
        }



        private static RadialChartVM GetRadialCartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            int increaseDecreaseRatio = 100;
            if (prevMonthCount != 0) {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }
            RadialChartVM radialChart = new() {
                TotalCount = totalCount,
                CountInCurrentMonth = Convert.ToInt32(currentMonthCount),
                HasRatioIncrease = currentMonthCount > prevMonthCount,
                Series = new int[] { increaseDecreaseRatio }
            };
            return radialChart;
        }

    }
}
