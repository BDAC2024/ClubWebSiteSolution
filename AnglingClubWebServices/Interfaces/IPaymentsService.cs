using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IPaymentsService
    {

        //List<Payment> GetPayments();
        Task<string> CreateCheckoutSession(CreateCheckoutSessionRequest createCheckoutSessionRequest);

        Task<OrderDetailDto> GetDetail(int orderId);

    }
}
