using Labor.DataAccess.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.Services.Telephony
{
    public class MockIvrService : IIvrService
    {
        private readonly ILogger<MockIvrService> _logger;
        public MockIvrService(ILogger<MockIvrService> logger)
        {
            _logger = logger;
        }
        public Task<string?> PlaceLaborConfirmationCallAsync(int laborConfirmationId, string laborMobile, string employerName, string workArea, DateTime? scheduledDate, CancellationToken ct = default)
        {
            _logger.LogWarning(
                 $"[MOCK IVR] Call LaborConfirmationId={laborConfirmationId} Mobile={laborMobile}. " +
            $"Kal aapko {workArea} mein kaam mila hai {employerName} ke yahan. 1=Haan, 2=Nahi, 9=Repeat. " +
            $"Test: POST /api/ivr/mock-response {{ laborConfirmationId: {laborConfirmationId}, digit: \"1\" }}"
            );
            return Task.FromResult<string?>($"mock-call-{laborConfirmationId}-{Guid.NewGuid():N}"
                );
                
        }
    }
}
