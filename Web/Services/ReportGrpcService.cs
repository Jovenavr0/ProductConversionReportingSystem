using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Grpc.Core;
using ReportServiceApp;
using ReportStatus = Domain.Enums.ReportStatus;

namespace Web.Services;

public class ReportGrpcService (IBillingService billingService, IOutboxService outboxService, IReportRequestService reportService) : ReportService.ReportServiceBase
{
   public override async Task<ReportResponse> RequestReport(ReportRequest request, ServerCallContext context)
    {
          var reportDto = MapReportRequest(request);
          
          var reportId = await outboxService.AddReportRequestAsync(reportDto);
          await billingService.InitializeReportRequest(request.UserId, reportId);
          
          return new ReportResponse { ReportId = reportId, Status = ReportServiceApp.ReportStatus.Pending };
    }
   
   public override async Task<ReportReply> GetReportResult(GetReportRequest request, ServerCallContext context)
   {
         var result = await reportService.Execute(request.ReportId);
         return new ReportReply { ReportId = result.ReportId, Status = MapStatus(result.ReportStatus) };
   }

   private static ReportRequestDto MapReportRequest(ReportRequest request)
   {
      return new ReportRequestDto(
         request.ProductId,
         request.StartGap.ToDateTime(),
         request.EndGap.ToDateTime(),
         request.UserId,
         request.DecorationId
      );
   }
   
   private static ReportServiceApp.ReportStatus MapStatus(ReportStatus status)
   {
      return status switch
      {
         ReportStatus.Pending => ReportServiceApp.ReportStatus.Pending,
         ReportStatus.Processing => ReportServiceApp.ReportStatus.Processing,
         ReportStatus.Completed => ReportServiceApp.ReportStatus.Completed,
         ReportStatus.Failed => ReportServiceApp.ReportStatus.Failed,
         _ => ReportServiceApp.ReportStatus.Pending
      };
   }
}