using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Payment;
using System.Data;

namespace Labor.DataAccess.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbContext _dbContext;

    public PaymentRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentSummaryDto?> GetPaymentSummaryAsync(int orderId, int employerId)
    {
        using var conn = _dbContext.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PaymentSummaryDto>(
            "[dbo].[sp_GetOrderPaymentSummary]",
            new { OrderID = orderId, EmployerID = employerId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreatePaymentRecordAsync(
        PaymentAmountBreakdown breakdown,
        int orderId,
        int employerId,
        string provider,
        string providerOrderId)
    {
        using var conn = _dbContext.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@OrderID", orderId);
        parameters.Add("@EmployerID", employerId);
        parameters.Add("@LaborAmount", breakdown.LaborAmount);
        parameters.Add("@PlatformFee", breakdown.PlatformFee);
        parameters.Add("@DiscountAmount", breakdown.DiscountAmount);
        parameters.Add("@CouponCode", breakdown.CouponCode);
        parameters.Add("@TotalAmount", breakdown.TotalAmount);
        parameters.Add("@Provider", provider);
        parameters.Add("@ProviderOrderId", providerOrderId);
        parameters.Add("@PaymentID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("[dbo].[sp_CreatePaymentRecord]", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<int>("@PaymentID");
    }

    public async Task<PaymentCompleteResultDto?> CompletePaymentAsync(
        int paymentId,
        int employerId,
        string providerPaymentId,
        string? providerSignature)
    {
        using var conn = _dbContext.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PaymentCompleteResultDto>(
            "[dbo].[sp_CompletePayment]",
            new
            {
                PaymentID = paymentId,
                EmployerID = employerId,
                ProviderPaymentId = providerPaymentId,
                ProviderSignature = providerSignature
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<EmployerContactDto?> GetEmployerContactForOrderAsync(int orderId)
    {
        using var conn = _dbContext.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<EmployerContactDto>(
            "[dbo].[sp_GetEmployerContactForOrder]",
            new { OrderID = orderId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task InsertNotificationLogAsync(
        int? userId,
        string mobile,
        string channel,
        string templateKey,
        string messageBody,
        string status)
    {
        using var conn = _dbContext.CreateConnection();
        await conn.ExecuteAsync(
            "[dbo].[sp_InsertNotificationLog]",
            new
            {
                UserID = userId,
                Mobile = mobile,
                Channel = channel,
                TemplateKey = templateKey,
                MessageBody = messageBody,
                Status = status
            },
            commandType: CommandType.StoredProcedure);
    }
}
