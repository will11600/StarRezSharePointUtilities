using StarRezTest.DataTypes;
using StarRezTest.HTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StarRezTest.HTTP
{
    internal class PlanetREST : IDisposable
    {
        private readonly PlanetHttpClientProivderCollection Providers = new();

        public CallDetails GetCallDetails(Report report)
        {
            HttpRequestMessage message = new(HttpMethod.Get, $"NonPMJobs/CallStatusDetail/{report.CallReferenceNumber}");
            using (var handler = Providers.Send(message, report.Room.Location))
            {
                CallDetails details = handler.ResponseHTML.ToObject<CallDetails>();
                details.ID = report.CallReferenceNumber;

                return details;
            }
        }

        public async Task<CallDetails> GetCallDetailsAsync(Report report)
        {
            HttpRequestMessage message = new(HttpMethod.Get, $"NonPMJobs/CallStatusDetail/{report.CallReferenceNumber}");
            using (var handler = Providers.Send(message, report.Room.Location, wait: false))
            {
                HTMLData responseHtml = await handler.GetResponseHTMLAsync();

                CallDetails details = responseHtml.ToObject<CallDetails>();
                details.ID = report.CallReferenceNumber;

                return details;
            }
        }

        public string GetCallReferenceNumber(Report report)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Providers.Dispose();
        }
    }
}
