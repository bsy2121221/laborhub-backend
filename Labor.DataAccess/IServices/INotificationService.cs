using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface INotificationService
    {
        Task NotifyEmployerLaborProgressAsync(int orderId);
        Task NotifyLaborConfirmedAsync(int orderId, int laborId);
        Task NotifyEmployerLaborDeclinedAsync(int orderId, int laborId);
        Task NotifyEmployerPaymentSuccessAsync(int orderId, string orderNumber, decimal amountPaid);
    }
}
