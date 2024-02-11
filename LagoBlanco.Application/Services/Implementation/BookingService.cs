using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;

namespace LagoBlanco.Application.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IEnumerable<Booking> GetAllBookings(string userId = "", string? statusListFilter = "")
        {
            //--- Truco EF para buscar un campo de BD, dentro de un array.
            IEnumerable<string> statusList = statusListFilter.ToLower().Split(",");
            //---
            if (!string.IsNullOrEmpty(statusListFilter) && !string.IsNullOrEmpty(userId)) {
                return _unitOfWork.Booking.GetAll(b => statusList.Contains(b.Status.ToLower()) &&
                                                       b.UserId == userId, "User, Villa");
            }
            else if (!string.IsNullOrEmpty(statusListFilter)) {
                return _unitOfWork.Booking.GetAll(b => statusList.Contains(b.Status.ToLower()), "User, Villa");
            }
            else if (!string.IsNullOrEmpty(userId)) {
                return _unitOfWork.Booking.GetAll(b => b.UserId == userId, "User, Villa");
            }
            else {
                return _unitOfWork.Booking.GetAll(includeProperties: "User, Villa");
            }                
        }


        public Booking GetBookingById(int bookingId)
        {
            return _unitOfWork.Booking.Get(b=>b.Id==bookingId,"User, Villa");
        }


        public void CreateBooking(Booking booking)
        {
            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();
        }


        public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber = 0)
        {
            var bookingDb = _unitOfWork.Booking.Get(b=>b.Id==bookingId, tracked:true);
            if (bookingDb != null) {
                bookingDb.Status = bookingStatus;
                if (bookingStatus == SD.StatusCheckedIn) {
                    bookingDb.VillaNumber = villaNumber;
                    bookingDb.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == SD.StatusCompleted) {
                    bookingDb.ActualCheckOutDate = DateTime.Now;
                }
                _unitOfWork.Save(); 
            }
        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingDb = _unitOfWork.Booking.Get(b => b.Id == bookingId, tracked:true);
            if (bookingDb != null) {
                if (!string.IsNullOrEmpty(sessionId)) {
                    bookingDb.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId)) {
                    bookingDb.StripePaymentIntentId = paymentIntentId;
                    bookingDb.PaymentDate = DateTime.Now;
                    bookingDb.IsPaymentSuccessful = true;
                }
                _unitOfWork.Save();
            }
        }

        public IEnumerable<int> GetCheckedVillaNumbers(int villaId)
        {   
            return _unitOfWork.Booking.GetAll(b=>b.VillaId==villaId && b.Status==SD.StatusCheckedIn)
                                      .Select(u=>u.VillaNumber);
        }
    }
}
