using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Security.Claims;

namespace LagoBlanco.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _repo;
        public BookingController(IUnitOfWork repo)
        {
            _repo = repo;
        }



        [Authorize]
        public IActionResult Index()
        {
            return View();
        }




        [Authorize]
        public IActionResult FinalizeBooking(int villaId, string checkInDate, int nights)
        {
            //Obtengo User de Bd con userId que me da el ClaimIdentity. 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = _repo.User.Get(u => u.Id == userId); 


            Booking booking = new() {
                VillaId = villaId, 
                CheckInDate = DateOnly.ParseExact(checkInDate, "dd/MM/yy"),                 
                Nights = nights,
                Villa = _repo.Villa.Get(v=>v.Id==villaId, includeProperties:"amenities"),
                //---
                UserId=userId, 
                Name=user.Name,
                Phone=user.PhoneNumber, 
                Email=user.Email
            };
            booking.CheckOutDate = booking.CheckInDate.AddDays(nights);
            booking.TotalCost = booking.Villa.Price * nights;
            //---
            return View(booking);
        }


        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {            
            var villa = _repo.Villa.Get(v => v.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;


            //---
            var villaNumbers = _repo.VillaNumber.GetAll().ToList();
            var bookedVillas = _repo.Booking.GetAll(b => b.Status == SD.StatusApproved ||
                                                         b.Status == SD.StatusCheckedIn).ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbers, booking.CheckInDate, 
                                                             booking.Nights, bookedVillas);
            if (roomAvailable== 0) {
                TempData["error"] = "Room has been sold out.";
                return RedirectToAction(nameof(FinalizeBooking), new {
                    villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights
                });
            }
            //---
            _repo.Booking.Add(booking);
            _repo.Save();

            //--- STRIPE
            var domain = Request.Scheme +  "://"+ Request.Host.Value + "/";
            var options = new SessionCreateOptions {
                LineItems = [], 
                Mode = "payment", //o suscription.
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl  = domain + $"booking/FinalizeBooking?int villaId={booking.VillaId}&"+
                                      $"checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            //---Puede haber un foreach, de un CartItems
            options.LineItems.Add(new SessionLineItemOptions {
                PriceData = new SessionLineItemPriceDataOptions {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions {
                        Name = villa.Name
                        //Images = new List<string> { domain + villa.ImageUrl },
                    },
                },
                Quantity = 1,
            });
            //---
            var service = new SessionService();
            Session session = service.Create(options);
            //---
            _repo.Booking.UpdateStripePaymentId(booking.Id, session.Id,session.PaymentIntentId);             
            _repo.Save(); 
            //---
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        [Authorize]        
        public IActionResult BookingConfirmation(int bookingId)
        {

            Booking bookingDb = _repo.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingDb.Status == SD.StatusPending) {
                var service = new SessionService();
                Session session = service.Get(bookingDb.StripeSessionId);
                if (session.PaymentStatus == "paid") {
                    _repo.Booking.UpdateStatus(bookingDb.Id, SD.StatusApproved, 0);
                    _repo.Booking.UpdateStripePaymentId(bookingDb.Id, session.Id, session.PaymentIntentId);
                    _repo.Save();
                }
            }
            return View(bookingId);
        }


        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingDb = _repo.Booking.Get(u => u.Id==bookingId, includeProperties: "User,Villa");

            if (bookingDb.VillaNumber == 0 && bookingDb.Status == SD.StatusApproved) {
                List<int> availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingDb.VillaId);

                bookingDb.VillaNumbers = _repo.VillaNumber.GetAll(u => 
                            u.VillaId==bookingDb.VillaId && 
                            availableVillaNumber.Any(x=>x==u.Villa_Number)).ToList();
            }
            return View(bookingDb);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _repo.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _repo.Save();
            TempData["Success"] = "Booking CheckIn Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _repo.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _repo.Save();
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _repo.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _repo.Save();
            TempData["Success"] = "Booking Canceled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }


        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = _repo.VillaNumber.GetAll(u => u.VillaId == villaId);

            var checkedInVilla = _repo.Booking.GetAll(u => u.VillaId == villaId && 
                                                           u.Status  == SD.StatusCheckedIn)
                                              .Select(u => u.VillaNumber);
            foreach (var villaNumber in villaNumbers) {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number)) 
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
            }
            return availableVillaNumbers;
        }



        #region Api Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;
            if (User.IsInRole(SD.Role_Admin)) 
            {   objBookings = _repo.Booking.GetAll(includeProperties:"User,Villa");
            }
            else {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objBookings = _repo.Booking.GetAll(b=>b.UserId==userId, includeProperties: "User,Villa");
            }
            if (!string.IsNullOrEmpty(status)) {
                objBookings = objBookings.Where(b=>b.Status.ToLower() == status.ToLower());
            }
            return Json(new {data=objBookings});
        }

        #endregion

    }
}
