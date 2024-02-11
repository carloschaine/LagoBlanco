using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Interface
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookings(string userId="", string? statusListFilter="");
        Booking GetBookingById(int bookingId);

        void CreateBooking(Booking booking);

        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId);

        public IEnumerable<int> GetCheckedVillaNumbers(int villaId); 
    }
}
