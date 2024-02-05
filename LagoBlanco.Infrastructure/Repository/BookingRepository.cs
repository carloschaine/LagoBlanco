using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Infrastructure.Repository
{

    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly AppDbContext _db;
        public BookingRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            var bookingDb = _db.Bookings.FirstOrDefault(m => m.Id == bookingId);
            if (bookingDb != null) {
                bookingDb.Status = bookingStatus;
                if (bookingStatus == SD.StatusCheckedIn) {
                    //bookingDb.VillaNumber = villaNumber;
                    bookingDb.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == SD.StatusCompleted) {
                    bookingDb.ActualCheckOutDate = DateTime.Now;
                }
            }
        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingDb = _db.Bookings.FirstOrDefault(m => m.Id == bookingId);
            if (bookingDb != null) {
                if (!string.IsNullOrEmpty(sessionId)) {
                    bookingDb.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId)) {
                    bookingDb.StripePaymentIntentId = paymentIntentId;
                    bookingDb.PaymentDate = DateTime.Now;
                    bookingDb.IsPaymentSuccessful = true;   
                }
            }
        }
    }

}
