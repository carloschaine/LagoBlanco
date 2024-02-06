using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.Utility
{
    public static class SD //Static Details
    {
        //AspNetRoles.Name
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";

        //Booking.Status
        public const string StatusPending   = "Pending";   //al crear el registro en la BD.
        public const string StatusApproved  = "Approved";  //Cuando Stripe me informa el pago exitoso.
        public const string StatusCheckedIn = "CheckedIn"; //Cuando el user llega al resort.
        public const string StatusCompleted = "Completed"; //Cuando hace el CheckOUt al irse.
        public const string StatusCancelled = "Cancelled"; //El usuario cancela la visita.
        public const string StatusRefunded  = "Refunded";  //Se reintegro importe despues de cancelar.

        public static int VillaRoomsAvailable_Count(int villaId, 
                                                    List<VillaNumber> villaNumberList, 
                                                    DateOnly checkInDate, int nights,
                                                    List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();

            for (int i = 0; i < nights; i++) {
                var villasBooked = bookings.Where(u => u.CheckInDate<=checkInDate.AddDays(i) &&
                                                       u.CheckOutDate>checkInDate.AddDays(i) && 
                                                       u.VillaId == villaId);
                foreach (var booking in villasBooked) {
                    if (!bookingInDate.Contains(booking.Id)) bookingInDate.Add(booking.Id);                    
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if (totalAvailableRooms == 0) {return 0;}
                else {
                    if (finalAvailableRoomForAllNights > totalAvailableRooms) {
                        finalAvailableRoomForAllNights = totalAvailableRooms;
                    }
                }
            }
            return finalAvailableRoomForAllNights;
        }
    }
}
