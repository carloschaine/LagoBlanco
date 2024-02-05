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
        //private readonly UserManager<ApplicationUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly SignInManager<ApplicationUser> _signInManager;
        public BookingController(IUnitOfWork repo
            //UserManager<ApplicationUser> userManager,
                                 //RoleManager<IdentityRole> roleManager,
                                 //SignInManager<ApplicationUser> signInManager
            )
        {
            _repo = repo;
            //_userManager = userManager; _roleManager = roleManager; _signInManager = signInManager;
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
                    _repo.Booking.UpdateStatus(bookingDb.Id, SD.StatusApproved);
                    _repo.Booking.UpdateStripePaymentId(bookingDb.Id, session.Id, session.PaymentIntentId);
                    _repo.Save();
                }
            }
            return View(bookingId);
        }



    }
}
