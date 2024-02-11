using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;


namespace LagoBlanco.Application.Services.Implementation
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IEnumerable<Amenity> GetAllAmenities()
        {
            return _unitOfWork.Amenity.GetAll(includeProperties:"Villa");
        }
        public IEnumerable<Amenity> GetAllAmenitiesByVillaId(int villaId)
        {
            return _unitOfWork.Amenity.GetAll(a=>a.VillaId==villaId, includeProperties: "Villa");
        }
        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.Amenity.Get(a=>a.Id==id, includeProperties: "Villa");
        }


        public void CreateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Save();
        }

        public void UpdateAmenity(Amenity amenity)
        {
            ArgumentNullException.ThrowIfNull(amenity);

            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try {
                Amenity? objDb = _unitOfWork.Amenity.Get(a => a.Id == id, tracked:true);
                if (objDb is not null) {
                    _unitOfWork.Amenity.Remove(objDb);
                    _unitOfWork.Save();
                    return true;
                }
                return false;
            }
            catch (Exception) {
                return false;
            }
        }


    }
}
