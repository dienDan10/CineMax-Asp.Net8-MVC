namespace DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IProvinceRepository Province { get; }
        ITheaterRepository Theater { get; }
        IMovieRepository Movie { get; }
        IShowTimeRepository ShowTime { get; }
        ISeatRepository Seat { get; }
        IScreenRepository Screen { get; }
        IBookingRepository Booking { get; }
        IBookingDetailRepository BookingDetail { get; }
        IConcessionRepository Concession { get; }
        IConcessionOrderRepository ConcessionOrder { get; }
        IConcessionOrderDetailRepository ConcessionOrderDetail { get; }
        IPaymentRepository Payment { get; }

        void Save();
    }
}
