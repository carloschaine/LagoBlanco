using LagoBlanco.Application.Common.DTO;
using LagoBlanco.Application.Common.DTO.Chart;
using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {

        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        static int previousYear = DateTime.Now.Year - DateTime.Now.Month == 1 ? 0 : 1;
        private readonly DateTime previousMonthStartDate = new(previousYear, previousMonth, 1);
        private readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);


        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public async Task<RadialChartDto> GetTotalBooking_ChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !(b.Status == SD.StatusPending ||
                                                                b.Status == SD.StatusCancelled));
            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate &&
                                                                u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate &&
                                                                u.BookingDate <= currentMonthStartDate);
            return GetRadialCartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }


        public async Task<RadialChartDto> GetRegisteredUser_ChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate &&
                                                             u.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate &&
                                                             u.CreatedAt <= currentMonthStartDate);
            return GetRadialCartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }


        public async Task<RadialChartDto> GetRevenue_ChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !(b.Status == SD.StatusPending ||
                                                               b.Status == SD.StatusCancelled));
            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));
            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate &&
                                                               u.BookingDate <= DateTime.Now)
                                                   .Sum(u => u.TotalCost);
            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate &&
                                                                u.BookingDate <= currentMonthStartDate)
                                                   .Sum(u => u.TotalCost);
            return GetRadialCartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }


        public async Task<PieChartDto> GetBooking_PieChart()
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
            PieChartDto pieChartVM = new() {
                Series = [bookingsByNewCustomer, bookingsByReturnedCustomer],
                Labels = ["New Customer", "Returned Customer"]
            };
            //--- 
            return pieChartVM;
        }

        public async Task<LineChartDto> GetMemberAndBooking_LineChart()
        {
            ChartData chartData1 = new() { Name = "Usuarios", Data = [4, 6, 5, 3] };
            ChartData chartData2 = new() { Name = "Reservas", Data = [7, 2, 4, 6] };
            //---
            LineChartDto lineChartVM = new LineChartDto() {
                Categories = ["1er semana", "2da semana", "3er semana", "4ta semana"],
                Series = [chartData1, chartData2]
            };
            //--- 
            return lineChartVM;
        }



        private static RadialChartDto GetRadialCartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            int increaseDecreaseRatio = 100;
            if (prevMonthCount != 0) {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }
            RadialChartDto radialChart = new() {
                TotalCount = totalCount,
                CountInCurrentMonth = Convert.ToInt32(currentMonthCount),
                HasRatioIncrease = currentMonthCount > prevMonthCount,
                Series = new int[] { increaseDecreaseRatio }
            };
            return radialChart;
        }

    }
}
