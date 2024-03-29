﻿using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;

using System.Security.Claims;

namespace LagoBlanco.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;//obtener path para Exports
        public BookingController(IBookingService bookingService, 
                                 IVillaService villaService,
                                 IVillaNumberService villaNumberService,
                                 UserManager<ApplicationUser> userManager,
                                 IWebHostEnvironment webHostEnvironment)
        {
            _bookingService = bookingService;
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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
            ApplicationUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult(); 

            Booking booking = new() {
                VillaId = villaId, 
                CheckInDate = DateOnly.ParseExact(checkInDate, "dd/MM/yy"),                 
                Nights = nights,
                Villa = _villaService.GetVillaById(villaId),
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
            var villa = _villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;
            //---
            
            if (!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate)) {
                TempData["error"] = "Room has been sold out.";
                return RedirectToAction(nameof(FinalizeBooking), new {
                    villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights
                });
            }
            //---
            _bookingService.CreateBooking(booking);
            //---

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
            _bookingService.UpdateStripePaymentId(booking.Id, session.Id,session.PaymentIntentId);                         
            //---
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        [Authorize]        
        public IActionResult BookingConfirmation(int bookingId)
        {

            Booking bookingDb = _bookingService.GetBookingById(bookingId);
            if (bookingDb.Status == SD.StatusPending) {
                var service = new SessionService();
                Session session = service.Get(bookingDb.StripeSessionId);
                if (session.PaymentStatus == "paid") {
                    _bookingService.UpdateStatus(bookingDb.Id, SD.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentId(bookingDb.Id, session.Id, session.PaymentIntentId);                   
                }
            }
            return View(bookingId);
        }


        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingDb = _bookingService.GetBookingById(bookingId);

            if (bookingDb.VillaNumber == 0 && bookingDb.Status == SD.StatusApproved) {
                List<int> availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingDb.VillaId);

                bookingDb.VillaNumbers = _villaNumberService.GetAllVillaNumbers()
                                    .Where(vn => availableVillaNumber.Any(x => x == vn.Villa_Number))
                                    .ToList();
                //availableVillaNumber viene filtrado por VillaId
                //bookingDb.VillaNumbers = _villaNumberService.GetAllVillaNumbers()
                //    .Where(vn => vn.VillaId==bookingDb.VillaId && 
                //            availableVillaNumber.Any(x=>x==vn.Villa_Number)).ToList();
            }
            return View(bookingDb);
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            //---
            TempData["Success"] = "Booking CheckIn Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            //---
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            //---            
            TempData["Success"] = "Booking Canceled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }





        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = [];
            var villaNumbers = _villaNumberService.GetAllVillaNumbersByVillaId(villaId);
            var checkedInVilla = _bookingService.GetCheckedVillaNumbers(villaId);
            //---
            foreach (var villaNumber in villaNumbers) {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number)) 
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
            }
            return availableVillaNumbers;
        }



        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {            
            //---
            WordDocument document = new WordDocument();
            
            // Load the template.
            string dataPath = _webHostEnvironment.WebRootPath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            //Update Template
            Booking bookingDb = _bookingService.GetBookingById(id);

            // En el doc.word agregamos "xx_" para identificar los campos a asignar. 
            TextSelection textSelection;
            WTextRange textRange; 
            //---           
            textSelection = document.Find("xx_customer_name", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.Name;

            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.Phone;

            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.Email;

            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID - " + bookingDb.Id;

            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingDb.BookingDate.ToShortDateString();

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.CheckOutDate.ToShortDateString(); ;

            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingDb.TotalCost.ToString("c");


            //---TABLA
            WTable table = new(document);
            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color =  Color.Black; //Color de Syncfusion, sino no funciona
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;
            int rows = bookingDb.VillaNumber > 0 ? 3 : 2;
            table.ResetCells(rows, 4);

            WTableRow row0 = table.Rows[0];
            row0.Cells[0].Width = 80;
            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[1].Width = 220;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].Width = 80;
            row0.Cells[3].AddParagraph().AppendText("TOTAL");

            WTableRow row1 = table.Rows[1];
            row1.Cells[0].Width = 80;
            row1.Cells[0].AddParagraph().AppendText(bookingDb.Nights.ToString());
            row1.Cells[1].Width = 220;
            row1.Cells[1].AddParagraph().AppendText(bookingDb.Villa.Name);
            row1.Cells[2].AddParagraph().AppendText((bookingDb.TotalCost / bookingDb.Nights).ToString("c"));
            row1.Cells[3].Width = 80;
            row1.Cells[3].AddParagraph().AppendText(bookingDb.TotalCost.ToString("c"));

            if (bookingDb.VillaNumber > 0) {
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].Width = 220;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingDb.VillaNumber.ToString());
                row2.Cells[3].Width = 80;
            }

            //--- Styles a la Table
            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles
                                                                 .Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.DarkOrange;
            table.ApplyStyle("CustomStyle");

            //Reemplazar en Template donde este: <ADDTABLEHERE>
            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);
            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);


            //--- IMPORTANTE: Guarda en formato .DOCX/.PDF y lo Retorna al Cliente.
            using DocIORenderer renderer = new();
            MemoryStream stream = new();
            //---
            if (downloadType == "word") {
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;
                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else {
                //Converts Word document into PDF document
                PdfDocument pdfDocument = renderer.ConvertToPDF(document);
                pdfDocument.Save(stream);
                stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");                
            }
        }



        #region Api Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {            
            status = string.IsNullOrEmpty(status) ? string.Empty : status;
            string userId = "";
            //---
            if (!User.IsInRole(SD.Role_Admin)) {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            //---
            IEnumerable<Booking> objBookings = _bookingService.GetAllBookings(userId, status);
            return Json(new {data=objBookings});
        }

        #endregion

    }
}
