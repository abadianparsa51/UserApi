using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UserApi.Data;
using UserApi.Models;
using UserApi.Models.DTOs;

namespace UserApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ApiDbContext context, ILogger<TransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost("Transfer")]
        public async Task<IActionResult> TransferMoney([FromBody] TransferRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.SourceCardNumber) || string.IsNullOrEmpty(request.DestinationCardNumber) || request.Amount <= 0)
            {
                return BadRequest("اطلاعات تراکنش معتبر نیست.");
            }

            string transactionId = Guid.NewGuid().ToString(); // شناسه تراکنش

            try
            {
                // بررسی کارت مبدا
                var sourceCard = await _context.CardDetails
                    .FirstOrDefaultAsync(c => c.CardNumber == request.SourceCardNumber);

                if (sourceCard == null)
                {
                    // ذخیره لاگ تراکنش در صورت عدم وجود کارت مبدا
                
                    return BadRequest("کارت مبدا یافت نشد.");
                }

                // بررسی پیشوند کارت مقصد
                var destinationCardPrefix = request.DestinationCardNumber.Substring(0, 6);
                var isValidPrefix = await _context.CardPrefixes
                    .AnyAsync(cp => cp.Prefix == destinationCardPrefix);

                if (!isValidPrefix)
                {
                    // ذخیره لاگ تراکنش در صورت عدم وجود پیشوند معتبر برای کارت مقصد
                  
                    return BadRequest("پیشوند کارت مقصد معتبر نیست.");
                }

                // فرض بر این است که کارمزد ثابت است
                decimal fee = 0.0005m * request.Amount; // کارمزد 0.05 درصد
                decimal amountAfterFee = request.Amount - fee;

                // بررسی موجودی کارت مبدا
                if (sourceCard.Balance < request.Amount)
                {
                    // ذخیره لاگ تراکنش در صورت عدم موجودی کافی
                  
                    return BadRequest("موجودی کافی نیست.");
                }

                // کاهش موجودی کارت مبدا
                sourceCard.Balance -= request.Amount;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // ذخیره TransactionLog
                        var transactionLog = new TransactionLog
                        {
                            TransactionId = transactionId,
                            SourceCardNumber = request.SourceCardNumber,
                            DestinationCardNumber = request.DestinationCardNumber,
                            Amount = request.Amount,
                            Fee = fee,
                            CardDetailId = sourceCard.Id,
                            TransactionDate = DateTime.UtcNow,
                            StatusMessage = " salam",
                            Status = "Success"
                        };
                        _context.TransactionLogs.Add(transactionLog);

                        // ذخیره Fee
                        var feeRecord = new Fee
                        {
                            FeeAmount = fee,
                            Date = DateTime.UtcNow,
                            SourceCardNumber = request.SourceCardNumber,
                            DestinationCardNumber = request.DestinationCardNumber,
                            TransactionId = transactionId
                        };
                        _context.Fees.Add(feeRecord);

                        await _context.SaveChangesAsync();

                        // تایید تراکنش
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during transaction.");
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                // لاگ سیستم
                _logger.LogInformation("Transaction logged: Source={Source}, Destination={Destination}, Amount={Amount}, Fee={Fee}",
                    request.SourceCardNumber, request.DestinationCardNumber, request.Amount, fee);

                return Ok(new
                {
                    Message = "تراکنش با موفقیت انجام شد.",
                    Fee = fee,
                    SourceCardBalance = sourceCard.Balance
                });
            }
            catch (Exception ex)
            {
                // ذخیره لاگ تراکنش در صورت بروز خطا
               
                _logger.LogError(ex, "خطا در انجام تراکنش.");
                return StatusCode(StatusCodes.Status500InternalServerError, "خطای داخلی سرور.");
            }
        }


        [HttpGet("balance/{cardNumber}")]
        public async Task<IActionResult> GetCardBalance(string cardNumber)
        {
            try
            {
                // بررسی اینکه آیا کارت در پایگاه داده وجود دارد
                var card = await _context.CardDetails
                    .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

                if (card == null)
                {
                    return BadRequest("کارت یافت نشد.");
                }

                // فرض بر این است که کارمزد ثابت است
                decimal fee = 0.0005m * card.Balance; // کارمزد 5 درصد از موجودی کارت
                decimal balanceAfterFee = card.Balance - fee;

                // ذخیره کارمزد در جدول Fee
                var feeRecord = new Fee
                {
                    FeeAmount = fee,
                    Date = DateTime.UtcNow,
                    SourceCardNumber = cardNumber,
                    DestinationCardNumber = cardNumber,
                    TransactionId = Guid.NewGuid().ToString() // شناسه تراکنش
                };

                // ذخیره کارمزد در دیتابیس
                _context.Fees.Add(feeRecord);

                // اعمال کارمزد بر موجودی کارت
                card.Balance -= fee;

                // ذخیره تغییرات در دیتابیس
                await _context.SaveChangesAsync();

                // بازگشت موجودی کارت و کارمزد به کاربر
                return Ok(new
                {
                    card.CardNumber,
                    Balance = balanceAfterFee,
                    Fee = fee
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطایی در حین دریافت موجودی کارت رخ داده است.");
                return StatusCode(StatusCodes.Status500InternalServerError, "خطای داخلی سرور.");
            }
        }

       

    }
}
