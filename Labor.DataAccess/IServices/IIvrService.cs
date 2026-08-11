using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface IIvrService
    {
        Task<string?> PlaceLaborConfirmationCallAsync(
       int laborConfirmationId,
       string laborMobile,
       string employerName,
       string workArea,
       DateTime? scheduledDate,
       CancellationToken ct = default);
    }
}
